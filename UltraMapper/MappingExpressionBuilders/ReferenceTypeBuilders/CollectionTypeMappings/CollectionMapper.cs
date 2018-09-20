using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CollectionMapper : ReferenceMapper
    {
        public CollectionMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsEnumerable() && target.IsEnumerable() &&
                !source.IsBuiltInType( false ) && !target.IsBuiltInType( false ); //avoid strings
        }

        protected override ReferenceMapperContext GetMapperContext( Type source, Type target, IMappingOptions options )
        {
            return new CollectionMapperContext( source, target, options );
        }

        protected virtual Expression SimpleCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar )
        {
            if( targetCollectionInsertionMethod == null )
            {
                string msg = $@"'{nameof( targetCollection.Type )}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var itemMapping = MapperConfiguration[ sourceCollectionElementType,
                targetCollectionElementType ].MappingExpression;

            Expression loopBody = Expression.Call
            (
                targetCollection, targetCollectionInsertionMethod,

                itemMapping.Body.ReplaceParameter( sourceCollectionLoopingVar,
                    itemMapping.Parameters[ 0 ].Name )
            );

            return ExpressionLoops.ForEach( sourceCollection,
                sourceCollectionLoopingVar, loopBody );
        }

        protected virtual Expression ComplexCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar,
            ParameterExpression referenceTracker, ParameterExpression mapper )
        {
            if( targetCollectionInsertionMethod == null )
            {
                string msg = $@"'{nameof( targetCollection.Type )}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var itemMapping = MapperConfiguration[ sourceCollectionLoopingVar.Type,
                targetCollectionElementType ].MappingExpression;

            var newElement = Expression.Variable( targetCollectionElementType, "newElement" );

            return Expression.Block
            (
                new[] { newElement },

                ExpressionLoops.ForEach( sourceCollection, sourceCollectionLoopingVar, Expression.Block
                (
                    Expression.IfThenElse
                    (
                        Expression.Equal( sourceCollectionLoopingVar, Expression.Constant( null, sourceCollectionElementType ) ),

                        Expression.Call( targetCollection, targetCollectionInsertionMethod, Expression.Default( targetCollectionElementType ) ),

                        Expression.Block
                        (
                            LookUpBlock( sourceCollectionLoopingVar, newElement, referenceTracker, mapper ),
                            Expression.Call( targetCollection, targetCollectionInsertionMethod, newElement )
                        )
                    )
                )
            ) );
        }

        protected BlockExpression LookUpBlock( ParameterExpression sourceParam, ParameterExpression targetParam,
            ParameterExpression referenceTracker, ParameterExpression mapper )
        {
            Expression itemLookupCall = Expression.Call
            (
                Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method,
                referenceTracker,
                sourceParam,
                Expression.Constant( targetParam.Type )
            );

            Expression itemCacheCall = Expression.Call
            (
                Expression.Constant( addToTracker.Target ),
                addToTracker.Method,
                referenceTracker,
                sourceParam,
                Expression.Constant( targetParam.Type ),
                targetParam
            );

            var mapMethod = CollectionMapperContext.RecursiveMapMethodInfo
                .MakeGenericMethod( sourceParam.Type, targetParam.Type );

            var itemMapping = MapperConfiguration[ sourceParam.Type, targetParam.Type ];

            return Expression.Block
            (
                Expression.Assign( targetParam, Expression.Convert( itemLookupCall, targetParam.Type ) ),

                Expression.IfThen
                (
                    Expression.Equal( targetParam, Expression.Constant( null, targetParam.Type ) ),

                    Expression.Block
                    (
                        Expression.Assign( targetParam, Expression.New( targetParam.Type ) ),

                        itemCacheCall,

                        Expression.Call( mapper, mapMethod, sourceParam, targetParam,
                            referenceTracker, Expression.Constant( itemMapping ) )
                    )
                )
            );
        }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            /* By default I try to retrieve the item-insertion method of the collection.
             * The exact name of the method can be overridden so that, for example, 
             * on Queue you search for 'Enqueue'. The default method name searched is 'Add'.
             * 
             * If the item-insertion method does not exist, try to retrieve a constructor
             * which takes as its only parameter 'IEnumerable<T>'. If this constructor
             * exists a temporary List<T> is created and then passed to the constructor.
             * 
             * If neither the item insertion method nor the above constructor exist
             * an exception is thrown
             */

            /* -Typically a Costructor(IEnumerable<T>) is faster than AddRange that is faster than Add.
             *  By the way Construcor( capacity ) + AddRange has roughly the same performance of Construcor( IEnumerable<T> ).             
             */

            bool isResetCollection = /*context.Options.ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL && */
                context.Options.CollectionBehavior == CollectionBehaviors.RESET;

            bool isUpdateCollection = context.Options.ReferenceBehavior == ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL &&
                context.Options.CollectionBehavior == CollectionBehaviors.UPDATE;

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null && isResetCollection )
            {
                string msg = $@"Cannot reset the collection. Type '{nameof( context.TargetInstance.Type )}' does not provide a Clear method";
                throw new Exception( msg );
            }

            var targetCollectionInsertionMethod = GetTargetCollectionInsertionMethod( context );

            if( context.IsSourceElementTypeBuiltIn || context.IsTargetElementTypeBuiltIn
                || context.IsSourceElementTypeStruct || context.IsTargetElementTypeStruct )
            {
                return Expression.Block
                (
                    isResetCollection ? Expression.Call( context.TargetInstance, clearMethod )
                        : (Expression)Expression.Empty(),

                    SimpleCollectionLoop( context.SourceInstance, context.SourceCollectionElementType,
                        context.TargetInstance, context.TargetCollectionElementType,
                        targetCollectionInsertionMethod, context.SourceCollectionLoopingVar )
                );
            }

            return Expression.Block
            (
                isResetCollection ? Expression.Call( context.TargetInstance, clearMethod )
                    : (Expression)Expression.Empty(),

                isUpdateCollection ? context.UpdateCollection
                    : ComplexCollectionLoop( context.SourceInstance, context.SourceCollectionElementType,
                        context.TargetInstance, context.TargetCollectionElementType,
                        targetCollectionInsertionMethod, context.SourceCollectionLoopingVar, context.ReferenceTracker, context.Mapper )
            );
        }

        /// <summary>
        /// Returns the method that allows to clear the target collection.
        /// </summary>
        protected virtual MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Clear ) );
        }

        /// <summary>
        /// Returns the method that allows to insert items in the target collection.
        /// </summary>
        protected virtual MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Add ) );
        }

        protected override Expression GetMemberNewInstance( MemberMappingContext context )
        {
            if( context.Options.CustomTargetConstructor != null )
                return Expression.Invoke( context.Options.CustomTargetConstructor );

            var collectionContext = new CollectionMapperContext( context.SourceMember.Type,
                context.TargetMember.Type, context.Options );

            //OPTIMIZATION: If the types involved are primitives of exactly the same type
            //we can use the constructor taking as input the collection and avoid recursion
            if( (collectionContext.IsSourceElementTypeBuiltIn || collectionContext.IsTargetElementTypeBuiltIn) &&
                collectionContext.SourceCollectionElementType == collectionContext.TargetCollectionElementType &&
                context.Options.ReferenceBehavior == ReferenceBehaviors.CREATE_NEW_INSTANCE )
            {
                var newInstance = GetNewInstanceFromSourceCollection( context, collectionContext );
                if( newInstance != null )
                {
                    var typeMapping = MapperConfiguration[ context.SourceMember.Type,
                        context.TargetMember.Type ];

                    //We do not want recursion on each collection's item
                    //but Capacity and other collection members must be mapped.
                    var memberMappings = this.GetMemberMappings( typeMapping )
                        .ReplaceParameter( context.Mapper, context.Mapper.Name )
                        .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                        .ReplaceParameter( context.SourceMember, context.SourceInstance.Name )
                        .ReplaceParameter( context.TargetMember, context.TargetInstance.Name );

                    context.InitializationComplete = true;

                    return Expression.Block
                    (
                        //in order to assign inner members we need to assign TargetMember
                        //(we also replaced TargetInstance with TargetMember)
                        Expression.Assign( context.TargetMember, newInstance ),
                        memberMappings,
                        context.TargetMember
                    );
                }
            }


            //OPTIMIZATION: if we need to create a new instance of a collection
            //we can try to reserve just the right capacity thus avoiding reallocations.
            //If the source collection implements ICollection we can read 'Count' property without any iteration.
            if( context.Options.ReferenceBehavior == ReferenceBehaviors.CREATE_NEW_INSTANCE
                && context.SourceMember.Type.ImplementsInterface( typeof( ICollection<> ) ) )
            {
                var newInstanceWithReservedCapacity = this.GetNewInstanceWithReservedCapacity( context );
                if( newInstanceWithReservedCapacity != null ) return newInstanceWithReservedCapacity;
            }


            //DEALING WITH INTERFACES
            Type sourceType = context.SourceMember.Type.IsGenericType ?
                 context.SourceMember.Type.GetGenericTypeDefinition() : context.SourceMember.Type;

            Type targetType = context.TargetMember.Type.IsGenericType ?
                    context.TargetMember.Type.GetGenericTypeDefinition() : context.TargetMember.Type;

            //If we are just cloning (ie: mapping on the same type) we prefer to use exactly the 
            //same runtime-type used in the source (in order to manage abstract classes, interfaces and inheritance). 
            if( context.TargetMember.Type.IsInterface && (context.TargetMember.Type.IsAssignableFrom( context.SourceMember.Type ) ||
                targetType.IsAssignableFrom( sourceType ) || sourceType.ImplementsInterface( targetType )) )
            {
                ////RUNTIME INSPECTION (in order to use on the target the same type of the source, if possible)
                ////MethodInfo getTypeMethodInfo = typeof( object ).GetMethod( nameof( object.GetType ) );
                ////var getSourceType = Expression.Call( context.SourceMemberValueGetter, getTypeMethodInfo );

                ////return Expression.Convert( Expression.Call( null, typeof( InstanceFactory ).GetMethods()[ 1 ],
                ////    getSourceType, Expression.Constant( null, typeof( object[] ) ) ), context.TargetMember.Type );

                //Runtime inspection did not work well between array and collection backed by ICollection or IEnumerable;
                //just provide a list if the target is backed by an interface...
                return Expression.New( typeof( List<> ).MakeGenericType( collectionContext.TargetCollectionElementType ) );
            }

            return Expression.New( context.TargetMember.Type );
        }

        /// <summary>
        /// Returns an expression calling Expression.New.
        /// Expression.New will call a constructor taking as input a collection
        /// </summary>
        protected virtual Expression GetNewInstanceFromSourceCollection( MemberMappingContext context, CollectionMapperContext collectionContext )
        {
            var targetConstructor = context.TargetMember.Type.GetConstructor(
               new[] { typeof( IEnumerable<> ).MakeGenericType( collectionContext.TargetCollectionElementType ) } );

            if( targetConstructor == null ) return null;
            return Expression.New( targetConstructor, context.SourceMember );
        }

        /// <summary>
        /// Returns an expression calling Expression.New.
        /// Expression.New will call a constructor intializing the capacity of the collection
        /// </summary>
        protected virtual Expression GetNewInstanceWithReservedCapacity( MemberMappingContext context )
        {
            var constructorWithCapacity = context.TargetMember.Type.GetConstructor( new Type[] { typeof( int ) } );
            if( constructorWithCapacity == null ) return null;

            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            var getCountProperty = context.SourceMember.Type.GetProperty( nameof( ICollection<int>.Count ),
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public );

            if( getCountProperty == null )
            {
                //ICollection<T> interface implementation is injected in the Array class at runtime.
                //Array implements ICollection.Count explicitly. For simplicity, we just look for property Length :)
                getCountProperty = context.SourceMember.Type.GetProperty( nameof( Array.Length ) );
            }

            var getCountMethod = getCountProperty.GetGetMethod();

            return Expression.New( constructorWithCapacity,
                Expression.Call( context.SourceMember, getCountMethod ) );
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Internals.ExtensionMethods;
using UltraMapper.ReferenceTracking;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CollectionMapper : ReferenceMapper
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;
            return source.EntryType.IsEnumerable() && target.EntryType.IsCollection();
        }

        protected override ReferenceMapperContext GetMapperContext( Mapping mapping )
        {
            return new CollectionMapperContext( mapping );
        }

        protected object RuntimeMappingInterfaceToPrimitiveType( object loopingvar, Type targetType, Configuration config )
        {
            var map = config[ loopingvar.GetType(), targetType ];
            return map.MappingFunc( null, loopingvar, null );
        }

        protected virtual Expression SimpleCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar,
            ParameterExpression mapper, ParameterExpression referenceTracker, CollectionMapperContext context )
        {
            if( targetCollectionInsertionMethod == null )
            {
                string msg = $@"'{targetCollection.Type}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            if( sourceCollectionElementType.IsInterface )
            {
                Expression<Func<object, Type, object>> getRuntimeMapping =
                   ( loopingvar, targetType ) => RuntimeMappingInterfaceToPrimitiveType( loopingvar, targetType, context.MapperConfiguration );

                var newElement = Expression.Variable( targetCollectionElementType, "newElement" );

                Expression loopBody = Expression.Block
                (
                    new[] { newElement },

                    Expression.Assign( newElement, Expression.Convert(
                            Expression.Invoke( getRuntimeMapping, sourceCollectionLoopingVar,
                            Expression.Constant( targetCollectionElementType ) ), targetCollectionElementType ) ),

                    Expression.Call( targetCollection, targetCollectionInsertionMethod, newElement )
                );

                return ExpressionLoops.ForEach( sourceCollection,
                    sourceCollectionLoopingVar, loopBody );
            }
            else
            {
                var itemMapping = context.MapperConfiguration[ sourceCollectionElementType,
                    targetCollectionElementType ].MappingExpression;

                Expression loopBody = Expression.Call
                (
                    targetCollection, targetCollectionInsertionMethod,
                    itemMapping.Body.ReplaceParameter( sourceCollectionLoopingVar, "sourceInstance" )
                );

                return ExpressionLoops.ForEach( sourceCollection,
                    sourceCollectionLoopingVar, loopBody );
            }
        }

        protected virtual Expression ComplexCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
            ParameterExpression targetCollection, Type targetCollectionElementType,
            MethodInfo targetCollectionInsertionMethod, ParameterExpression sourceCollectionLoopingVar,
            ParameterExpression referenceTracker, ParameterExpression mapper, CollectionMapperContext context = null )
        {
            if( targetCollectionInsertionMethod == null )
            {
                string msg = $@"'{targetCollection.Type}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var newElement = Expression.Variable( targetCollectionElementType, "newElement" );

            //var mapping = ((Mapping)context.Options).GlobalConfig[ sourceCollectionElementType, targetCollectionElementType ];
            //if( mapping.Source.EntryType != mapping.Source.ReturnType )
            //    mapping = ((Mapping)context.Options).GlobalConfig[ mapping.Source.ReturnType, targetCollectionElementType ];

            //var valueGetter = mapping.Source.ValueGetter;

            ///*member extraction support*/
            //Expression valueExtraction = Expression.Invoke( valueGetter, sourceCollectionLoopingVar );
            //if( ((Mapping)context.Options).Source.MemberAccessPath.Count <= 1 )
            //    valueExtraction = sourceCollectionLoopingVar;

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
                            LookUpBlock( sourceCollectionLoopingVar, newElement, referenceTracker, mapper, context ),
                            Expression.Call( targetCollection, targetCollectionInsertionMethod, newElement )
                        )
                    )
                )
            ) );
        }

        protected Expression LookUpBlock( ParameterExpression sourceParam, ParameterExpression targetParam,
            ParameterExpression referenceTracker, ParameterExpression mapper, CollectionMapperContext context )
        {
            var itemMapping = context.MapperConfiguration[ sourceParam.Type, targetParam.Type ];

            var constructorExp = base.GetMemberNewInstanceInternal( sourceParam,
                sourceParam.Type, targetParam.Type, itemMapping );

            var memberAssignment = Expression.Assign( targetParam, constructorExp );

            if( itemMapping.IsReferenceTrackingEnabled )
            {
                return ReferenceTrackingExpression.GetMappingExpression(
                    referenceTracker, sourceParam, targetParam,
                    memberAssignment, mapper, null, itemMapping,
                    Expression.Constant( itemMapping ) );
            }
            else
            {
                ////non recursive
                //return Expression.Block
                //(
                //      memberAssignment,
                //      Expression.Invoke( itemMapping.MappingExpression, referenceTracker, sourceParam, targetParam )
                //);

                //recursive
                var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                    .MakeGenericMethod( sourceParam.Type, targetParam.Type );

                return Expression.Block
                (
                    memberAssignment,

                    Expression.Call( mapper, mapMethod, sourceParam, targetParam,
                        referenceTracker, Expression.Constant( itemMapping ) )
                );
            }
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

            //reset only if we keep using the same target instance
            bool isResetCollection = context.Options.ReferenceBehavior != ReferenceBehaviors.CREATE_NEW_INSTANCE
                && context.Options.CollectionBehavior == CollectionBehaviors.RESET;

            bool isUpdateCollection = /*context.Options.ReferenceBehavior == ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL &&*/
                context.Options.CollectionBehavior == CollectionBehaviors.UPDATE;

            var targetCollectionInsertionMethod = GetTargetCollectionInsertionMethod( context );

            //if( context.SourceCollectionElementType.IsInterface )
            //{
            //    throw new NotImplementedException( "collections of interface element type not fully supported yet" );
            //    //especially interface -> simple type
            //}

            if( (context.IsSourceElementTypeBuiltIn || context.IsTargetElementTypeBuiltIn
                || context.SourceCollectionElementType.IsValueType || context.TargetCollectionElementType.IsValueType) )
            {
                //var ctor = context.TargetInstance.Type.GetConstructor( new Type[] { typeof( int ) } );

                return Expression.Block
                (
                    //Expression.IfThen
                    //(
                    //    Expression.Equal( context.TargetInstance, context.TargetInstanceNullValue ),
                    //    Expression.Assign( context.TargetInstance, Expression.New( ctor, Expression.Constant( 10 ) ) )
                    //),

                    isResetCollection ? GetTargetCollectionClearExpression( context ) : Expression.Empty(),

                    SimpleCollectionLoop( context.SourceInstance, context.SourceCollectionElementType,
                        context.TargetInstance, context.TargetCollectionElementType,
                        targetCollectionInsertionMethod, context.SourceCollectionLoopingVar,
                        context.Mapper, context.ReferenceTracker, context )
                );
            }

            return Expression.Block
            (
                isResetCollection ? GetTargetCollectionClearExpression( context ) : Expression.Empty(),

                isUpdateCollection ? GetUpdateCollectionExpression( context )
                    : ComplexCollectionLoop
                      (
                        context.SourceInstance,
                        context.SourceCollectionElementType,
                        context.TargetInstance,
                        context.TargetCollectionElementType,
                        targetCollectionInsertionMethod,
                        context.SourceCollectionLoopingVar,
                        context.ReferenceTracker,
                        context.Mapper,
                        context
                      )
            );
        }

        /// <summary>
        /// Returns the method that allows to clear the target collection.
        /// </summary>
        protected virtual MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            if( context.TargetInstance.Type.IsArray )
            {
                return typeof( Array ).GetMethod( nameof( Array.Clear ),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy );
            }

            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Clear ) );
        }

        /// <summary>
        /// Returns the expression that clears the collection
        /// </summary>
        protected virtual Expression GetTargetCollectionClearExpression( CollectionMapperContext context )
        {
            bool isResetCollection = /*context.Options.ReferenceBehavior == ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL &&*/
                context.Options.CollectionBehavior == CollectionBehaviors.RESET;

            if( isResetCollection )
            {
                var clearMethod = GetTargetCollectionClearMethod( context );
                if( clearMethod == null && isResetCollection )
                {
                    string msg = $@"Cannot reset the collection. Type '{context.TargetInstance.Type}' does not provide a Clear method";
                    throw new Exception( msg );
                }

                return Expression.Call( context.TargetInstance, clearMethod );
            }

            return Expression.Empty();
        }

        /// <summary>
        /// Returns the method that allows to insert items in the target collection.
        /// </summary>
        protected virtual MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Add ) );
        }

        public override Expression GetMemberNewInstance( MemberMappingContext context, out bool isMapCompleted )
        {
            isMapCompleted = false;

            if( context.Options.CustomTargetConstructor != null )
                return Expression.Invoke( context.Options.CustomTargetConstructor );

            var collectionContext = new CollectionMapperContext( (Mapping)context.Options );

            if( context.TargetMember.Type.IsArray )
            {
                var sourceCountMethod = GetCountMethod( context.MemberMapping.SourceMember.ReturnType );

                Expression sourceCountMethodCallExp;
                if( sourceCountMethod.IsStatic )
                    sourceCountMethodCallExp = Expression.Call( null, sourceCountMethod, context.SourceMember );
                else sourceCountMethodCallExp = Expression.Call( context.SourceMemberValueGetter, sourceCountMethod );

                var ctorArgTypes = new[] { typeof( int ) };
                var ctorInfo = context.TargetMember.Type.GetConstructor( ctorArgTypes );

                return Expression.New( ctorInfo, sourceCountMethodCallExp );
            }

            //OPTIMIZATION: If the types involved are primitives of exactly the same type
            //we can use the constructor taking as input the collection and avoid recursion
            if( (collectionContext.IsSourceElementTypeBuiltIn || collectionContext.IsTargetElementTypeBuiltIn) &&
                collectionContext.SourceCollectionElementType == collectionContext.TargetCollectionElementType &&
                context.Options.ReferenceBehavior == ReferenceBehaviors.CREATE_NEW_INSTANCE )
            {
                var newInstance = GetNewInstanceFromSourceCollection( context, collectionContext );
                if( newInstance != null )
                {
                    var typeMapping = context.MapperConfiguration[ context.SourceMember.Type,
                        context.TargetMember.Type ];


                    //We do not want recursion on each collection's item
                    //but Capacity and other collection members must be mapped.
                    Expression memberMappings = Expression.Empty();

                    if( context.TargetMemberValueGetter != null ) //we can only map subparam if a way to access subparam is provided/resolved. Edge case is: providing a member's setter method but not the getter's 
                    {
                        memberMappings = this.GetMemberMappingsExpression( typeMapping )
                            .ReplaceParameter( context.Mapper, context.Mapper.Name )
                            .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                            .ReplaceParameter( context.SourceMember, context.SourceInstance.Name )
                            .ReplaceParameter( context.TargetMember, context.TargetInstance.Name );
                    }

                    isMapCompleted = true; //we created a new instance also passing source array of non-reference type that will be copied
                    return Expression.Block
                    (
                        Expression.IfThenElse
                        (
                            Expression.IsTrue( Expression.Equal( context.SourceMemberValueGetter, context.SourceMemberNullValue ) ),

                            //Expression.Assign( context.TargetMember, context.TargetMemberNullValue ), //this only works for properties/fields
                            context.TargetMemberValueSetter
                                .ReplaceParameter( context.TargetMemberNullValue, "targetValue" ), //works on setter methods too

                            Expression.Block
                            (
                                //in order to assign inner members we need to assign TargetMember
                                //(we also replaced TargetInstance with TargetMember)

                                context.TargetMemberValueSetter
                                    .ReplaceParameter( newInstance, "targetValue" ), //works on setter methods too

                                //Expression.Assign( context.TargetMember, newInstance ), //this only works for properties/fields

                                memberMappings ?? Expression.Empty()
                            )
                        )
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

            var defaultCtor = targetType.GetConstructor( Type.EmptyTypes );
            if( defaultCtor != null )
                return Expression.New( context.TargetMember.Type );

            if( targetType.IsInterface )
            {
                //use List<> as default collection type
                return Expression.New( typeof( List<> ).MakeGenericType( collectionContext.TargetCollectionElementType ) );
            }

            throw new Exception( $"Type {targetType} does not have a default constructor. " +
                $"Please provide a way to construct the type like this: cfg.MapTypes<A, B>( () => new B(param1,param2,...) ) " );
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

            var getCountMethod = GetCountMethod( context.SourceMember.Type );

            Expression getCountMethodCallExp;
            if( getCountMethod.IsStatic )
                getCountMethodCallExp = Expression.Call( null, getCountMethod, context.SourceMember );
            else getCountMethodCallExp = Expression.Call( context.SourceMember, getCountMethod );

            return Expression.New( constructorWithCapacity, getCountMethodCallExp );
        }

        protected virtual MethodInfo GetUpdateCollectionMethod( CollectionMapperContext context )
        {
            return typeof( LinqExtensions ).GetMethod
            (
                nameof( LinqExtensions.Update ),
                BindingFlags.Static | BindingFlags.Public
            )
            .MakeGenericMethod
            (
                context.SourceCollectionElementType,
                context.TargetCollectionElementType
            );
        }

        protected virtual Expression GetUpdateCollectionExpression( CollectionMapperContext context )
        {
            if( context.Options.CollectionItemEqualityComparer == null )
                return Expression.Empty();

            var updateCollectionMethodInfo = GetUpdateCollectionMethod( context );

            return Expression.Call( null, updateCollectionMethodInfo, context.Mapper,
               context.ReferenceTracker, context.SourceInstance, context.TargetInstance,
               Expression.Convert( Expression.Constant( context.Options.CollectionItemEqualityComparer.Compile() ),
                    typeof( Func<,,> ).MakeGenericType( context.SourceCollectionElementType, context.TargetCollectionElementType, typeof( bool ) ) ) );
        }

        protected virtual MethodInfo GetCountMethod( Type type )
        {
            return GetCountMethodStatic( type );
        }

        public static MethodInfo GetCountMethodStatic( Type collectionType )
        {
            var getCountMethod = collectionType.GetMethod( nameof( ICollection<int>.Count ),
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy );

            if( getCountMethod != null )
                return getCountMethod;

            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            var getCountProperty = collectionType.GetProperty( nameof( ICollection<int>.Count ),
               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy );

            if( getCountProperty != null )
                return getCountProperty.GetGetMethod();

            if( collectionType.IsArray )
            {
                //ICollection<T> interface implementation is injected in the Array class at runtime.
                //Array implements ICollection.Count explicitly. For simplicity, we just look for property Length :)
                getCountProperty = collectionType.GetProperty( nameof( Array.Length ) );

                if( getCountProperty != null )
                    return getCountProperty.GetGetMethod();
            }

            var getLinqCount = typeof( System.Linq.Enumerable ).GetMethods(
                    BindingFlags.Static | BindingFlags.Public )
                .FirstOrDefault( m =>
                {
                    if( m.Name != nameof( System.Linq.Enumerable.Count ) )
                        return false;

                    var parameters = m.GetParameters();
                    if( parameters.Length != 1 ) return false;

                    return parameters[ 0 ].ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> );
                } );

            //Trying to retrieve the elementType from the collection is not always possible
            //in case of an instance of an unmaterialized enumerable.
            //In that case here we are working with RangeIterator, SelectorIterator, WhereIterator etc...
            //From some of them (for example RangeIterator) is not possible to retrieve the collection element type.

            //DO NOT USE THIS
            var elementTypes = collectionType.GetGenericArguments();
            if( elementTypes.Any() )
            {
                var genericCount = getLinqCount?.MakeGenericMethod( elementTypes[ 0 ] );

                if( genericCount != null )
                    return genericCount;
            }

            throw new ArgumentException( $"Type '{collectionType}' does not define a Count or Length property and Linq.Count could not be used" );
        }
    }
}

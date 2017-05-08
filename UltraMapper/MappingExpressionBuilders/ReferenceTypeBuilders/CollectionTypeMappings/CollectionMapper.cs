using System;
using System.Collections.Generic;
using System.Linq;
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

        public virtual Expression ComplexCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
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
                    LookUpBlock( sourceCollectionLoopingVar, newElement, referenceTracker, mapper ),
                    Expression.Call( targetCollection, targetCollectionInsertionMethod, newElement )
                )
            ) );
        }

        public BlockExpression LookUpBlock( ParameterExpression sourceParam, ParameterExpression targetParam,
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
                context.Options.CollectionMappingStrategy == CollectionMappingStrategies.RESET;

            bool isUpdateCollection = context.Options.ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL &&
                context.Options.CollectionMappingStrategy == CollectionMappingStrategies.UPDATE;

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null && isResetCollection )
            {
                string msg = $@"Cannot reset the collection. Type '{nameof( context.TargetInstance.Type )}' does not provide a Clear method";
                throw new Exception( msg );
            }

            var targetCollectionInsertionMethod = GetTargetCollectionInsertionMethod( context );

            if( context.IsSourceElementTypeBuiltIn || context.IsTargetElementTypeBuiltIn )
            {
                ////OPTIMIZATION: If the types involved are primitives of exactly the same type
                ////and we need to create a new instance,
                ////we can use the constructor taking as input the collection and avoid looping

                //if( context.SourceCollectionElementType == context.TargetCollectionElementType &&
                //    context.Options.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                //{
                //    var targetConstructor = context.TargetInstance.Type.GetConstructor(
                //        new[] { typeof( IEnumerable<> ).MakeGenericType( context.TargetCollectionElementType ) } );
                //    if( targetConstructor != null )
                //    {
                //        //CANNOT ASSIGN THE INSTANCE: WE LOSE THE REFERENCE!
                //        return Expression.New( targetConstructor, context.SourceInstance );
                //    }
                //}

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

        public override Expression GetTargetInstanceAssignment( MemberMappingContext context )
        {
            var collectionContext = new CollectionMapperContext( context.SourceMember.Type,
                context.TargetMember.Type, context.Options );
            if( collectionContext.IsSourceElementTypeBuiltIn || collectionContext.IsTargetElementTypeBuiltIn )
            {
                //OPTIMIZATION: If the types involved are primitives of exactly the same type
                //and we need to create a new instance,
                //we can use the constructor taking as input the collection and avoid looping

                if( collectionContext.SourceCollectionElementType == collectionContext.TargetCollectionElementType &&
                    context.Options.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                {
                    var targetConstructor = context.TargetInstance.Type.GetConstructor(
                        new[] { typeof( IEnumerable<> ).MakeGenericType( collectionContext.TargetCollectionElementType ) } );

                    if( targetConstructor != null )
                    {
                        context.NeedRecursion = false;
                        return Expression.New( targetConstructor, context.SourceMember );
                    }
                }
            }

            //OPTIMIZATION: if we need to create a new instance of a collection
            //we can try to reserve just the right capacity thus avoiding reallocations.
            //If the source collection implements ICollection we can read 'Count' property without any iteration.

            if( context.Options.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE
                && context.SourceMember.Type.ImplementsInterface( typeof( ICollection<> ) ) )
            {
                var constructorWithCapacity = context.TargetMember.Type.GetConstructor( new Type[] { typeof( int ) } );
                if( constructorWithCapacity != null )
                {
                    //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
                    var getCountProperty = context.SourceMember.Type.GetProperty( nameof( ICollection<int>.Count ),
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public );
                    if( getCountProperty == null )
                    {
                        //ICollection<T> interface implementation is injected in the Array class at runtime.
                        //Array implements ICollection.Count explicitly. 
                        //For simplicity, we just look for property Length :)
                        getCountProperty = context.SourceMember.Type.GetProperty( nameof( Array.Length ) );
                    }

                    var getCountMethod = getCountProperty.GetGetMethod();

                    return Expression.Assign( context.TargetMember, Expression.New( constructorWithCapacity,
                        Expression.Call( context.SourceMember, getCountMethod ) ) );
                }
            }

            return base.GetTargetInstanceAssignment( context );
        }

        protected override Expression GetNewTargetInstance( MemberMappingContext context )
        {
            if( context.Options.CustomTargetConstructor != null )
                return Expression.Invoke( context.Options.CustomTargetConstructor );

            Type sourceType = context.SourceMember.Type.IsGenericType ?
                 context.SourceMember.Type.GetGenericTypeDefinition() : context.SourceMember.Type;

            Type targetType = context.TargetMember.Type.IsGenericType ?
                    context.TargetMember.Type.GetGenericTypeDefinition() : context.TargetMember.Type;

            if( context.TargetMember.Type.IsInterface && (context.TargetMember.Type.IsAssignableFrom( context.SourceMember.Type ) ||
                targetType.IsAssignableFrom( sourceType ) || sourceType.ImplementsInterface( targetType )) )
            {
                var collectionContext = new CollectionMapperContext( context.SourceMember.Type,
                    context.TargetMember.Type, context.Options );

                var createInstanceMethodInfo = typeof( Activator )
                    .GetMethods( BindingFlags.Static | BindingFlags.Public )
                    .Where( method => method.Name == nameof( Activator.CreateInstance ) )
                    .Select( method => new
                    {
                        Method = method,
                        Params = method.GetParameters(),
                        Args = method.GetGenericArguments()
                    } )
                    .Where( x => x.Params.Length == 0 && x.Args.Length == 1 )
                    .Select( x => x.Method )
                    .First();

                MethodInfo getType = typeof( object ).GetMethod( nameof( object.GetType ) );
                MethodInfo getGenericTypeDefinition = typeof( Type ).GetMethod( nameof( Type.GetGenericTypeDefinition ) );

                var getSourceTypeGenericDefinition = Expression.Call( Expression.Call( context.SourceMemberValueGetter, getType ), getGenericTypeDefinition );
                var makeGenericMethod = typeof( MethodInfo ).GetMethod( nameof( MethodInfo.MakeGenericMethod ) );
                var makeGenericType = typeof( Type ).GetMethod( nameof( Type.MakeGenericType ) );

                var arrayParameter = Expression.Parameter( typeof( List<Type> ), "pararray" );
                var arrayParameter2 = Expression.Parameter( typeof( List<Type> ), "pararray2" );

                var toArray = typeof( Enumerable )
                    .GetMethod( nameof( Enumerable.ToArray ) )
                    .MakeGenericMethod( new[] { typeof( Type ) } );

                var addType = typeof( List<Type> ).GetMethod( nameof( List<Type>.Add ) );

                var parArray = Expression.Call( null, toArray, arrayParameter );
                var parArray2 = Expression.Call( null, toArray, arrayParameter2 );

                var createInstance = Expression.Call( Expression.Constant( createInstanceMethodInfo ), makeGenericMethod, parArray );

                return Expression.Block
                (
                    new[] { arrayParameter, arrayParameter2 },

                    Expression.Assign( arrayParameter, Expression.New( typeof( List<Type> ) ) ),
                    Expression.Assign( arrayParameter2, Expression.New( typeof( List<Type> ) ) ),

                    Expression.Call( arrayParameter2, addType, Expression.Constant( collectionContext.TargetCollectionElementType ) ),
                    Expression.Call( arrayParameter, addType, Expression.Call( getSourceTypeGenericDefinition, makeGenericType, parArray2 ) ),

                    Expression.Invoke( debugExp, arrayParameter ),
                    Expression.Invoke( debugExp, arrayParameter2 ),

                    Expression.Convert(
                       Expression.Call( createInstance, typeof( MethodInfo ).GetMethod( nameof( MethodInfo.Invoke ), new[] { typeof( object ), typeof( object[] ) } ),
                       Expression.Constant( null ), Expression.Constant( null, typeof( object[] ) ) ), context.TargetMember.Type )
                );
            }

            return Expression.New( context.TargetMember.Type );
        }
    }
}


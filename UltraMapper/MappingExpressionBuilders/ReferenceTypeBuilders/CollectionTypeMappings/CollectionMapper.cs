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

        protected Expression SimpleCollectionLoop( ParameterExpression sourceCollection, Type sourceCollectionElementType,
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

        public Expression CollectionLoopWithReferenceTracking( ParameterExpression sourceCollection, Type sourceCollectionElementType,
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
                    : CollectionLoopWithReferenceTracking( context.SourceInstance, context.SourceCollectionElementType,
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
            //A little optimization: if we need to create a new instance of a collection
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
    }
}


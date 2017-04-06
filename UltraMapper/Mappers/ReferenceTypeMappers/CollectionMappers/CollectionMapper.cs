using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Mappers.MapperContexts;

namespace UltraMapper.Mappers
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

        protected Expression SimpleCollectionLoop( CollectionMapperContext context,
            ParameterExpression sourceCollection, ParameterExpression targetCollection )
        {
            var targetCollectionInsertionMethod = GetTargetCollectionInsertionMethod( context );
            if( targetCollectionInsertionMethod == null )
            {
                string msg = $@"'{nameof( context.TargetInstance.Type )}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var itemMapping = MapperConfiguration[ context.SourceCollectionElementType,
                context.TargetCollectionElementType ].MappingExpression;

            Expression loopBody = Expression.Call
            (
                targetCollection, targetCollectionInsertionMethod,
                itemMapping.Body.ReplaceParameter(
                    context.SourceCollectionLoopingVar, itemMapping.Parameters[ 0 ].Name )
            );

            return ExpressionLoops.ForEach( sourceCollection,
                context.SourceCollectionLoopingVar, loopBody );
        }

        public Expression CollectionLoopWithReferenceTracking( CollectionMapperContext context,
            ParameterExpression sourceCollection, ParameterExpression targetCollection )
        {
            var targetCollectionInsertionMethod = GetTargetCollectionInsertionMethod( context );
            if( targetCollectionInsertionMethod == null )
            {
                string msg = $@"'{nameof( context.TargetInstance.Type )}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var itemMapping = MapperConfiguration[ context.SourceCollectionLoopingVar.Type,
                context.TargetCollectionElementType ].MappingExpression;

            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );

            return Expression.Block
            (
                new[] { newElement },

                ExpressionLoops.ForEach( sourceCollection, context.SourceCollectionLoopingVar, Expression.Block
                (
                    LookUpBlock( context, context.SourceCollectionLoopingVar, newElement ),
                    Expression.Call( targetCollection, targetCollectionInsertionMethod, newElement )
                )
            ) );
        }

        public BlockExpression LookUpBlock( CollectionMapperContext context,
            ParameterExpression sourceParam, ParameterExpression targetParam )
        {
            Expression itemLookupCall = Expression.Call
            (
                Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method,
                context.ReferenceTracker,
                sourceParam,
                Expression.Constant( targetParam.Type )
            );

            Expression itemCacheCall = Expression.Call
            (
                Expression.Constant( addToTracker.Target ),
                addToTracker.Method,
                context.ReferenceTracker,
                sourceParam,
                Expression.Constant( targetParam.Type ),
                targetParam
            );

            var mapMethod = context.RecursiveMapMethodInfo
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

                        Expression.Call( context.Mapper, mapMethod, sourceParam, targetParam,
                            context.ReferenceTracker, Expression.Constant( itemMapping ) )
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

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null && context.Options.CollectionMappingStrategy == CollectionMappingStrategies.RESET )
            {
                string msg = $@"Cannot reset the collection. Type '{nameof( context.TargetInstance.Type )}' does not provide a Clear method";
                throw new Exception( msg );
            }

            if( context.IsSourceElementTypeBuiltIn || context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    context.Options.CollectionMappingStrategy == CollectionMappingStrategies.RESET ?
                        Expression.Call( context.TargetInstance, clearMethod ) : (Expression)Expression.Empty(),

                    SimpleCollectionLoop( context, context.SourceInstance, context.TargetInstance )
                );
            }

            return Expression.Block
            (
                context.Options.CollectionMappingStrategy == CollectionMappingStrategies.RESET ?
                    Expression.Call( context.TargetInstance, clearMethod ) : (Expression)Expression.Empty(),

                CollectionLoopWithReferenceTracking( context, context.SourceInstance, context.TargetInstance )
            );
        }

        /// <summary>
        /// Returns the method that allows to clear the target collection.
        /// </summary>
        private MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Clear ) );
        }

        protected override Expression ReturnListInitialization( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            var getCountMethod = context.SourceInstance.Type.GetProperty( "Count" ).GetGetMethod();

            return Expression.Assign( context.ReturnObject, Expression.New( context.ReturnTypeConstructor,
                Expression.Call( context.SourceInstance, getCountMethod ) ) );
        }

        /// <summary>
        /// Returns the method that allows to insert items in the target collection.
        /// </summary>
        protected virtual MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Add ) );
        }

        public override Expression GetTargetInstanceAssignment( MemberMappingContext context, MemberMapping mapping )
        {
            if( mapping.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE
                && context.SourceInstance.Type.ImplementsInterface( typeof( ICollection<> ) )
                && context.TargetInstance.Type.ImplementsInterface( typeof( ICollection<> ) ) )
            {
                var constructorWithCapacity = context.TargetInstance.Type.GetConstructor( new Type[] { typeof( int ) } );
                if( constructorWithCapacity != null )
                {
                    //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
                    var getCountMethod = context.SourceInstance.Type.GetProperty( nameof( ICollection<int>.Count ) ).GetGetMethod();
                    return Expression.Assign( context.TargetInstance, Expression.New( constructorWithCapacity,
                        Expression.Call( context.SourceInstance, getCountMethod ) ) );
                }
            }

            return base.GetTargetInstanceAssignment( context, mapping );
        }
    }
}


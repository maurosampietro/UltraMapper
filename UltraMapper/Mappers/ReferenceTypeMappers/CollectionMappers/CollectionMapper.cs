using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Configuration;
using UltraMapper.Internals;
using UltraMapper.Mappers.MapperContexts;

namespace UltraMapper.Mappers
{
    public class CollectionMapper : ReferenceMapper
    {
        public static UltraMapper _runtimeRecursor;
        protected bool NeedsImmediateRecursion { get; set; }

        public CollectionMapper( MapperConfiguration configuration )
            : base( configuration )
        {
            this.NeedsImmediateRecursion = false;
            _runtimeRecursor = new UltraMapper( configuration );
        }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsEnumerable() && target.IsEnumerable() &&
                !source.IsBuiltInType( false ) && !target.IsBuiltInType( false ); //avoid strings
        }

        protected override ReferenceMapperContext GetMapperContext( Type source, Type target )
        {
            return new CollectionMapperContext( source, target );
        }

        protected Expression SimpleCollectionLoop( CollectionMapperContext context,
            ParameterExpression sourceCollection, ParameterExpression targetCollection )
        {
            var targetCollectionAddMethod = GetTargetCollectionInsertionMethod( context );
            if( targetCollectionAddMethod == null )
            {
                string msg = $@"'{nameof( context.TargetInstance.Type )}' does not provide an insertion method. " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var itemMapping = MapperConfiguration[ context.SourceCollectionElementType,
                context.TargetCollectionElementType ].MappingExpression;

            Expression loopBody = Expression.Call
            (
                targetCollection, targetCollectionAddMethod,
                itemMapping.Body.ReplaceParameter(
                    context.SourceCollectionLoopingVar, itemMapping.Parameters[ 0 ].Name )
            );

            return ExpressionLoops.ForEach( sourceCollection,
                context.SourceCollectionLoopingVar, loopBody );
        }

        protected Expression CollectionLoopWithReferenceTracking( CollectionMapperContext context,
            ParameterExpression sourceCollection, ParameterExpression targetCollection )
        {
            var targetCollectionAddMethod = GetTargetCollectionInsertionMethod( context );
            if( targetCollectionAddMethod == null )
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
                    LookUpBlock( itemMapping, context, context.ReferenceTracker, context.SourceCollectionLoopingVar, newElement ),
                    Expression.Call( targetCollection, targetCollectionAddMethod, newElement )
                )
            ) );
        }

        public static MethodInfo getTypeMapperMapGenericMethod()
        {
            return typeof( UltraMapper ).GetMethods()
                .Where( m => m.Name == "Map" )
                .Select( m => new
                {
                    Method = m,
                    Params = m.GetParameters(),
                    GenericArgs = m.GetGenericArguments()
                } )
                .Where( x => x.Params.Length == 3
                            && x.GenericArgs.Length == 2
                            && x.Params[ 0 ].ParameterType == x.GenericArgs[ 0 ] &&
                             x.Params[ 1 ].ParameterType == x.GenericArgs[ 1 ] &&
                             x.Params[ 2 ].ParameterType == typeof( ReferenceTracking ) )
                .Select( x => x.Method )
                .First();
        }

        protected BlockExpression LookUpBlock( LambdaExpression itemMapping, CollectionMapperContext context,
            ParameterExpression referenceTracker, ParameterExpression sourceParam, ParameterExpression targetParam )
        {
            Expression itemLookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, referenceTracker, Expression.Convert( sourceParam, typeof( object ) ),
                    Expression.Constant( targetParam.Type ) );

            Expression itemCacheCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, referenceTracker, Expression.Convert( sourceParam, typeof( object ) ),
                Expression.Constant( targetParam.Type ), Expression.Convert( targetParam, typeof( object ) ) );

            var recursor = Expression.Variable( _runtimeRecursor.GetType(), "recursor" );
            var mapMethod = getTypeMapperMapGenericMethod().MakeGenericMethod( sourceParam.Type, targetParam.Type );

            Expression cacheItemAndRecurseImmediately = Expression.Block
            (
                new[] { recursor },

                itemCacheCall,

                Expression.Assign( recursor, Expression.Constant( _runtimeRecursor ) ),
                Expression.Call( recursor, mapMethod, sourceParam, targetParam, referenceTracker )
            );

            Expression deferItemRecursion = Expression.Call
            (
                context.ReturnObject, context.AddObjectPairToReturnList,
                Expression.New( context.ReturnElementConstructor, sourceParam, targetParam )
            );

            return Expression.Block
            (
                Expression.Assign( targetParam, Expression.Convert( itemLookupCall, targetParam.Type ) ),

                Expression.IfThen
                (
                    Expression.Equal( targetParam, Expression.Constant( null, targetParam.Type ) ),

                    Expression.Block
                    (
                        Expression.Assign( targetParam, Expression.New( targetParam.Type ) ),
                        NeedsImmediateRecursion ? cacheItemAndRecurseImmediately : deferItemRecursion
                    )
                )
            );
        }

        protected override Expression GetExpressionBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            /* By default try to retrieve the item-insertion method of the collection.
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
             * 
             * -Must also manage the case where SourceElementType and TargetElementType differ:
             *  cannot use directly the target constructor: use add method or temp collection.
             */

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetInstance.Type )}' does not provide a clear method";
                throw new Exception( msg );
            }

            if( context.IsTargetElementTypeBuiltIn )
            {
                return Expression.Block
                (
                    Expression.Call( context.TargetInstance, clearMethod ),
                    SimpleCollectionLoop( context, context.SourceInstance, context.TargetInstance )
                );
            }

            return Expression.Block
            (
                Expression.Call( context.TargetInstance, clearMethod ),
                CollectionLoopWithReferenceTracking( context, context.SourceInstance, context.TargetInstance )
            );
        }

        protected override Expression ReturnListInitialization( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            var getCountMethod = context.SourceInstance.Type.GetProperty( "Count" ).GetGetMethod();

            return Expression.Assign( context.ReturnObject, Expression.New( context.ReturnTypeConstructor
                , Expression.Call( context.SourceInstance, getCountMethod ) ) );
        }

        /// <summary>
        /// Returns the method that allows to insert items in the target collection.
        /// </summary>
        protected virtual MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Add ) );
        }

        /// <summary>
        /// Returns the method that allows to clear the target collection.
        /// </summary>
        private MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
            return context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Clear ) );
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


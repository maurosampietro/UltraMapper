using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    /*NOTES:
     * 
     *- Collections that do not implement ICollection<T> must specify which method
     *to use to 'Add' an item or must have a constructor that takes as param an IEnumerable.
     * 
     */

    public class CollectionMapper : ReferenceMapper
    {
        protected bool NeedsImmediateRecursion { get; set; }

        public CollectionMapper( MapperConfiguration configuration )
            : base( configuration ) { this.NeedsImmediateRecursion = false; }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsEnumerable() && target.IsEnumerable() &&
                !source.IsBuiltInType( false ) && !target.IsBuiltInType( false ); //avoid strings
        }

        protected override ReferenceMapperContext GetMapperContext( Type source, Type target )
        {
            return new CollectionMapperContext( source, target );
        }

        protected virtual Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            //- Typically a Costructor(IEnumerable<T>) is faster than AddRange that is faster than Add.
            //  By the way Construcor(capacity) + AddRange has roughly the same performance of Construcor(IEnumerable<T>). 
            //- Must also manage the case where SourceElementType and TargetElementType differ:
            //  cannot use directly the target constructor: use add method or temp collection.

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetInstance.Type )}' does not provide a clear method";
                throw new Exception( msg );
            }

            var addMethod = GetTargetCollectionInsertionMethod( context );
            if( addMethod == null )
            {
                string msg = $@"Cannot use existing instance on target object. '{nameof( context.TargetInstance.Type )}' does not provide an item-insertion method " +
                    $"Please override '{nameof( GetTargetCollectionInsertionMethod )}' to provide the item-insertion method.";

                throw new Exception( msg );
            }

            return Expression.Block
            (
                Expression.Call( context.TargetInstance, clearMethod ),
                SimpleCollectionLoop( context, context.SourceInstance, context.TargetInstance )
            );
        }

        protected virtual Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            /*
             * By default try to retrieve the item-insertion method of the collection.
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

            var typeMapping = MapperConfiguration[ context.SourceCollectionElementType,
                context.TargetCollectionElementType ];

            //var targetInstanceAssignment = GetTargetInstanceAssignment( context );

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetInstance.Type )}' does not provide a clear method";
                throw new Exception( msg );
            }

            return Expression.Block
            (
                Expression.Call( context.TargetInstance, clearMethod ),
                CollectionLoopWithReferenceTracking( context, context.SourceInstance, context.TargetInstance )
            );
        }

        private MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            return context.TargetInstance.Type.GetMethod( "Clear" );
        }

        protected virtual Expression SimpleCollectionLoop( CollectionMapperContext context, ParameterExpression sourceCollection,
            ParameterExpression targetCollection )
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
                Expression.Invoke( itemMapping, context.SourceCollectionLoopingVar )
            );

            return ExpressionLoops.ForEach( sourceCollection,
                context.SourceCollectionLoopingVar, loopBody );
        }

        protected virtual Expression CollectionLoopWithReferenceTracking( CollectionMapperContext context,
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

        protected BlockExpression LookUpBlock( LambdaExpression itemMapping, CollectionMapperContext context,
            ParameterExpression referenceTracker, ParameterExpression sourceParam, ParameterExpression targetParam )
        {
            Expression cacheLookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, referenceTracker, Expression.Convert( sourceParam, typeof( object ) ),
                    Expression.Constant( targetParam.Type ) );

            Expression cacheInsertCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, referenceTracker, Expression.Convert( sourceParam, typeof( object ) ),
                Expression.Constant( targetParam.Type ), Expression.Convert( targetParam, typeof( object ) ) );

            Expression cacheItemAndRecurseImmediately = Expression.Block
            (
                cacheInsertCall,

                itemMapping.Body
                    .ReplaceParameter( referenceTracker, referenceTracker.Name )
                    .ReplaceParameter( sourceParam, "sourceInstance" )
                    .ReplaceParameter( targetParam, "targetInstance" )
            );

            Expression deferItemRecursion = Expression.Call
            (
                context.ReturnObject, context.AddObjectPairToReturnList,
                Expression.New( typeof( ObjectPair ).GetConstructors()[ 0 ],
                Expression.Convert( sourceParam, typeof( object ) ),
                Expression.Convert( targetParam, typeof( object ) ) )
            );

            return Expression.Block
            (
                Expression.Assign( targetParam, Expression.Convert( cacheLookupCall, targetParam.Type ) ),

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

        protected override Expression GetInnerBody( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            if( context.IsTargetElementTypeBuiltIn )
                return GetSimpleTypeInnerBody( context );

            return GetComplexTypeInnerBody( context );
        }

        protected override Expression ReturnListInitialization( ReferenceMapperContext contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            var getCountMethod = context.SourceInstance.Type.GetProperty( "Count" ).GetGetMethod();

            return Expression.Assign( context.ReturnObject, Expression.New( context.ReturnTypeConstructor,
                Expression.Call( context.SourceInstance, getCountMethod ) ) );
        }

        /// <summary>
        /// Return the method that allows to add items to the target collection.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetTargetCollectionInsertionMethod( CollectionMapperContext context )
        {
            return context.TargetInstance.Type.GetMethod( "Add" );
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
                    //ICollection<int> is used only because it is forbidden to use nameof with unbound generic types.
                    //Any other type instead of int would work.
                    var getCountMethod = context.SourceInstance.Type.GetProperty( nameof( ICollection<int>.Count ) ).GetGetMethod();
                    return Expression.Assign( context.TargetInstance, Expression.New( constructorWithCapacity,
                        Expression.Call( context.SourceInstance, getCountMethod ) ) );
                }
            }

            return base.GetTargetInstanceAssignment( context, mapping );
        }
    }
}


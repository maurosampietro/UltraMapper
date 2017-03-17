using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Configuration;
using TypeMapper.Internals;

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
        public CollectionMapper( GlobalConfiguration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsEnumerable() && target.IsEnumerable();
        }

        protected override object GetMapperContext( MemberMapping mapping )
        {
            return new CollectionMapperContext( mapping );
        }

        protected virtual Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            //- Typically a Costructor(IEnumerable<T>) is faster than AddRange that is faster than Add.
            //- Must also manage the case where SourceElementType and TargetElementType differ:
            // cannot use directly the target constructor: use add method or temp collection.

            var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
            if( constructorInfo == null || MapperConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL
                    || context.SourceCollectionElementType != context.TargetCollectionElementType )
            {
                var addMethod = GetTargetCollectionAddMethod( context );
                if( addMethod == null )
                {
                    string msg = $@"Cannot use existing instance on target object. '{nameof( context.TargetMemberType )}' does not provide an item-insertion method " +
                        $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item-insertion method.";

                    throw new Exception( msg );
                }

                var typeMapping = MapperConfiguration.Configurator[
                          context.SourceCollectionElementType, context.TargetCollectionElementType ];

                var convert = typeMapping.MappingExpression;

                Expression loopBody = Expression.Call( context.TargetMember,
                    addMethod, Expression.Invoke( convert, context.SourceCollectionLoopingVar ) );

                var targetInstanceExpression = GetTargetInstanceAssignment( context );

                return Expression.Block
                (
                    targetInstanceExpression,

                    ExpressionLoops.ForEach( context.SourceMember,
                        context.SourceCollectionLoopingVar, loopBody )
                );
            }

            var constructor = GetTargetCollectionConstructorFromCollection( context );
            var targetCollectionConstructor = Expression.New( constructor, context.SourceMember );

            return Expression.Assign( context.TargetMember, targetCollectionConstructor );
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

            var targetInstanceAssignment = GetTargetInstanceAssignment( context );

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod != null || MapperConfiguration
                .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
            {
                return Expression.Block
                (
                    targetInstanceAssignment,
                    CollectionLoopWithReferenceTracking( context, context.TargetMember, addMethod )
                );
            }

            //Look for the constructor which takes an initial collection as parameter
            var constructor = GetTargetCollectionConstructorFromCollection( context );
            if( constructor == null )
            {
                string msg = $@"'{nameof( context.TargetMemberType )}' does not provide an 'Add' method or a constructor taking as parameter IEnumerable<T>. " +
                    $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var tempCollectionType = typeof( List<> ).MakeGenericType( context.TargetCollectionElementType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );
            var tempCollectionAddMethod = tempCollectionType.GetMethod( "Add" );

            var tempCtorWithCapacity = tempCollectionType.GetConstructor( new Type[] { typeof( int ) } );
            var tempCollectionCountMethod = context.SourceMemberType.GetProperty( "Count" ).GetGetMethod();

            var newTempCollectionExp = Expression.New( tempCtorWithCapacity,
                Expression.Call( context.SourceMember, tempCollectionCountMethod ) );

            return Expression.Block
            (
                new[] { tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                CollectionLoopWithReferenceTracking( context, tempCollection, tempCollectionAddMethod ),
                Expression.Assign( context.TargetMember, Expression.New( constructor, tempCollection ) )
            );
        }

        protected virtual Expression CollectionLoopWithReferenceTracking( CollectionMapperContext context,
            ParameterExpression targetCollection, MethodInfo targetCollectionAddMethod )
        {
            var itemMapping = MapperConfiguration.Configurator
                [ context.SourceCollectionLoopingVar.Type, context.TargetCollectionElementType ].MappingExpression;

            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );

            return Expression.Block
            (
                new[] { newElement },

                ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                (
                    LookUpBlock( itemMapping, context.ReferenceTrack, context.SourceCollectionLoopingVar, newElement ),
                    Expression.Call( targetCollection, targetCollectionAddMethod, newElement )
                )
            ) );
        }

        protected BlockExpression LookUpBlock( LambdaExpression itemMapping, ParameterExpression referenceTracker,
           Expression sourceParam, ParameterExpression targetParam )
        {
            Expression lookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, referenceTracker, sourceParam,
                    Expression.Constant( targetParam.Type ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, referenceTracker, sourceParam,
                Expression.Constant( targetParam.Type ), targetParam );

            return Expression.Block
            (
                Expression.Assign( targetParam, Expression.Convert( lookupCall, targetParam.Type ) ),

                Expression.IfThen
                (
                    Expression.Equal( targetParam, Expression.Constant( null, targetParam.Type ) ),

                    Expression.Block
                    (
                        Expression.Assign( targetParam, Expression.New( targetParam.Type ) ),

                        //cache new collection
                        addToLookupCall,

                        Expression.Invoke( itemMapping, referenceTracker,
                            sourceParam, targetParam )
                    )
                )
            );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            if( context.IsTargetElementTypeBuiltIn )
                return GetSimpleTypeInnerBody( context );

            return GetComplexTypeInnerBody( context );
        }

        protected virtual ConstructorInfo GetTargetCollectionConstructorFromCollection( CollectionMapperContext context )
        {
            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.TargetCollectionElementType ) };

            return context.TargetMemberType.GetConstructor( paramType );
        }

        /// <summary>
        /// Return the method that allows to add items to the target collection.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetMemberType.GetMethod( "Add" );
        }

        protected override Expression GetTargetInstanceAssignment( object contextObj )
        {
            var context = contextObj as CollectionMapperContext;
            var typeMapping = MapperConfiguration.Configurator[
                context.SourceMember.Type, context.TargetMember.Type ];

            if( typeMapping.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE
                && context.SourceMemberType.ImplementsInterface( typeof( ICollection<> ) )
                && context.TargetMemberType.ImplementsInterface( typeof( ICollection<> ) ) )
            {
                var constructorWithCapacity = context.TargetMemberType.GetConstructor( new Type[] { typeof( int ) } );
                if( constructorWithCapacity != null )
                {
                    //ICollection<int> is used only because it is forbidden to use nameof with unbound generic types.
                    //Any other type instead of int would work.
                    var getCountMethod = context.SourceMemberType.GetProperty( nameof( ICollection<int>.Count ) ).GetGetMethod();
                    return Expression.Assign( context.TargetMember, Expression.New( constructorWithCapacity,
                        Expression.Call( context.SourceMember, getCountMethod ) ) );
                }
            }

            return base.GetTargetInstanceAssignment( contextObj );
        }
    }
}


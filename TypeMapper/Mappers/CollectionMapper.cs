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

    public class CollectionMapper : ReferenceMapper, IMemberMappingMapperExpression
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            return mapping.SourceMember.MemberType.IsEnumerable() &&
                 mapping.TargetMember.MemberType.IsEnumerable();
        }

        //public bool CanHandle( TypeMapping mapping )
        //{
        //    return mapping.TypePair.SourceType.IsEnumerable() &&
        //         mapping.TypePair.TargetType.IsEnumerable();
        //}

        protected override object GetMapperContext( MemberMapping mapping )
        {
            return new CollectionMapperContext( mapping );
        }

        //protected object GetMapperContext( TypeMapping mapping )
        //{
        //    return new CollectionMapperContext( mapping );
        //}

        protected virtual Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            /*If reference mapping strategy is USE_TARGET_INSTANCE_IF_NOT_NULL
             * we can only use the item-insertion method to map cause we are not allowed not create new instances.
             */

            Func<Expression> GetTargetInstanceExpression = () =>
            {
                if( context.MapperConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
                {
                    return context.TargetMemberValue;
                }

                return Expression.New( context.TargetMemberType );
            };

            //- Typically a Costructor(IEnumerable<T>) is faster than AddRange that is faster than Add.
            //- Must also manage the case where SourceElementType and TargetElementType differ:
            // cannot use directly the target constructor: use add method or temp collection.

            var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
            if( constructorInfo == null || context.MapperConfiguration
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

                var typeMapping = context.MapperConfiguration.Configurator[
                          context.SourceCollectionElementType, context.TargetCollectionElementType ];

                var convert = typeMapping.MappingExpression;

                Expression loopBody = Expression.Call( context.TargetMember,
                    addMethod, Expression.Invoke( convert, context.SourceCollectionLoopingVar ) );

                var targetInstanceExpression = GetTargetInstanceExpression();

                return Expression.Block
                (
                    Expression.Assign( context.TargetMember, targetInstanceExpression ),

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
            Func<Expression> GetTargetInstanceExpression = () =>
            {
                if( context.MapperConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
                {
                    return context.TargetMemberValue;
                }

                var newInstanceExp = Expression.New( context.TargetMemberType );
                if( context.TargetMemberType.ImplementsInterface( typeof( ICollection<> ) ) )
                {
                    var constructorWithCapacity = context.TargetMemberType.GetConstructor( new Type[] { typeof( int ) } );
                    if( constructorWithCapacity != null )
                    {
                        var getCountMethod = context.SourceMemberType.GetProperty( "Count" ).GetGetMethod();
                        newInstanceExp = Expression.New( constructorWithCapacity, Expression.Call( context.SourceMember, getCountMethod ) );
                    }
                }

                return newInstanceExp;
            };

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

            //var objectPairConstructor = context.ReturnElementType.GetConstructors().First();

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod != null || context.MapperConfiguration
                .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
            {
                return Expression.Block
                (
                    Expression.Assign( context.TargetMember, GetTargetInstanceExpression() ),
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
            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );

            return Expression.Block
            (
                new[] { newElement },

                ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                (
                    LookUpBlock( context, context.ReferenceTrack, context.SourceCollectionLoopingVar, newElement ),
                    Expression.Call( targetCollection, targetCollectionAddMethod, newElement )
                )
            ) );
        }

        protected BlockExpression LookUpBlock( CollectionMapperContext context, ParameterExpression referenceTracker,
           Expression sourceParam, ParameterExpression targetParam )
        {
            var itemMapping = context.MapperConfiguration.Configurator
             [ sourceParam.Type, targetParam.Type ].MappingExpression;

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
    }
}


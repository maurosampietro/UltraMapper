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
     *to use to 'Add' an item or must have a constructor that takes as param a IEnumerable.
     * 
     */

    public class CollectionMapper : ReferenceMapper, IObjectMapperExpression
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            return mapping.SourceProperty.IsEnumerable &&
                 mapping.TargetProperty.IsEnumerable;
        }

        protected override object GetMapperContext( MemberMapping mapping )
        {
            return new CollectionMapperContext( mapping );
        }

        protected virtual Expression GetSimpleTypeInnerBody( MemberMapping mapping, CollectionMapperContext context )
        {
            /*If reference mapping strategy is USE_TARGET_INSTANCE_IF_NOT_NULL
             * we can only use the item-insertion method to map cause we are not allowed not create new instances.
             */

            Func<Expression> GetTargetInstanceExpression = () =>
            {
                if( context.Mapping.TypeMapping.GlobalConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
                {
                    return context.TargetMemberValue;
                }

                return Expression.New( context.TargetMemberType );
            };

            //- Typically a Costructor(IEnumerable<T>) is faster than AddRange that is faster than Add.
            //- Must also need the case where SourceElementType and TargetElementType differ:
            // cannot use directly the target constructor: use add method or temp collection.

            var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
            if( constructorInfo == null || context.Mapping.TypeMapping.GlobalConfiguration
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

                var typeMapping = mapping.TypeMapping.GlobalConfiguration.Configurator[
                          context.SourceCollectionElementType, context.TargetCollectionElementType ];

                var convert = MappingExpressionBuilderFactory.GetMappingExpression
                    ( typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );

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

        protected virtual Expression GetComplexTypeInnerBody( MemberMapping mapping, CollectionMapperContext context )
        {
            Func<Expression> GetTargetInstanceExpression = () =>
            {
                if( context.Mapping.TypeMapping.GlobalConfiguration
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

            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );

            Expression lookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, context.ReferenceTrack, context.SourceCollectionLoopingVar,
                    Expression.Constant( context.TargetCollectionElementType ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, context.ReferenceTrack, context.SourceCollectionLoopingVar,
                Expression.Constant( context.TargetCollectionElementType ), newElement );

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod != null || context.Mapping.TypeMapping.GlobalConfiguration
                .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
            {
                return Expression.Block
                (
                    new[] { newElement },

                    Expression.Assign( context.TargetMember, GetTargetInstanceExpression() ),
                    ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                    (
                        Expression.Assign( newElement, Expression.Convert( lookupCall, context.TargetCollectionElementType ) ),                       
                        Expression.IfThen
                        (
                            Expression.Equal( newElement, Expression.Constant( null, context.TargetCollectionElementType ) ),
                            Expression.Block
                            (
                                Expression.Assign( newElement, Expression.New( context.TargetCollectionElementType ) ),

                                //cache new collection
                                addToLookupCall,

                                //add to return list
                                Expression.Call( context.ReturnObject, addToRefCollectionMethod,
                                    Expression.New( objectPairConstructor, context.SourceCollectionLoopingVar, newElement ) )
                            )
                        ),
                        
                        Expression.Call( context.TargetMember, addMethod, newElement )
                    ) )
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
                new[] { newElement, tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                (
                    Expression.Assign( newElement, Expression.Convert( lookupCall, context.TargetCollectionElementType ) ),

                    Expression.IfThen
                    (
                        Expression.Equal( newElement, Expression.Constant( null, context.TargetCollectionElementType ) ),
                        Expression.Block
                        (
                            Expression.Assign( newElement, Expression.New( context.TargetCollectionElementType ) ),

                            //cache new collection
                            addToLookupCall,

                            Expression.Call( context.ReturnObject, addToRefCollectionMethod,
                                Expression.New( objectPairConstructor, context.SourceCollectionLoopingVar, newElement ) )
                        )
                    ),

                    Expression.Call( tempCollection, tempCollectionAddMethod, newElement )             
                ) ),

                Expression.Assign( context.TargetMember, Expression.New( constructor, tempCollection ) )
            );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            if( context.IsTargetElementTypeBuiltIn )
                return GetSimpleTypeInnerBody( context.Mapping, context );

            return GetComplexTypeInnerBody( context.Mapping, context );
        }

        protected override Expression ReturnTypeInitialization( object contextObj )
        {
            var context = contextObj as CollectionMapperContext;
            return Expression.Assign( context.ReturnObject,
                Expression.New( context.ReturnType ) );
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


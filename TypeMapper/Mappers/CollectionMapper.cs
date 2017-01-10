using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        public override bool CanHandle( PropertyMapping mapping )
        {
            return mapping.SourceProperty.IsEnumerable &&
                 mapping.TargetProperty.IsEnumerable;
        }

        protected override object GetMapperContext( PropertyMapping mapping )
        {
            return new CollectionMapperContext( mapping );
        }

        protected virtual Expression GetSimpleTypeInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            /*If reference mapping strategy is USE_TARGET_INSTANCE_IF_NOT_NULL
             * we can only use the item-insertion method to map cause we are not allowed not create new instances.
             */

            Func<Expression> GetTargetInstanceExpression = () =>
            {
                if( context.Mapping.TypeMapping.GlobalConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
                {
                    return context.Mapping.TargetProperty.ValueGetter
                        .Body.ReplaceParameter( context.TargetInstance );
                }

                return Expression.New( context.TargetPropertyType );
            };

            var constructorInfo = GetTargetCollectionConstructorFromCollection( context );
            if( constructorInfo == null || context.Mapping.TypeMapping.GlobalConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
            {
                var addMethod = GetTargetCollectionAddMethod( context );
                if( addMethod == null )
                {
                    string msg = $@"Cannot use existing instance on target object. '{nameof( context.TargetPropertyType )}' does not provide an item-insertion method " +
                        $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item-insertion method.";

                    throw new Exception( msg );
                }

                Expression loopBody = Expression.Call( context.TargetPropertyVar,
                    addMethod, context.SourceLoopingVar );

                return Expression.Block
                (
                    Expression.Assign( context.TargetPropertyVar, GetTargetInstanceExpression() ),

                    ExpressionLoops.ForEach( context.SourcePropertyVar,
                        context.SourceLoopingVar, loopBody )
                );
            }

            var constructor = GetTargetCollectionConstructorFromCollection( context );
            var targetCollectionConstructor = Expression.New( constructor, context.SourcePropertyVar );

            return Expression.Assign( context.TargetPropertyVar, targetCollectionConstructor );
        }

        protected virtual Expression GetComplexTypeInnerBody( PropertyMapping mapping, CollectionMapperContext context )
        {
            Func<Expression> GetTargetInstanceExpression = () =>
            {
                if( context.Mapping.TypeMapping.GlobalConfiguration
                    .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
                {
                    return context.Mapping.TargetProperty.ValueGetter.Body.ReplaceParameter( context.TargetInstance );
                }

                var newInstanceExp = Expression.New( context.TargetPropertyType );
                if( context.TargetPropertyType.ImplementsInterface( typeof( ICollection<> ) ) )
                {
                    var constructorWithCapacity = context.TargetPropertyType.GetConstructor( new Type[] { typeof( int ) } );
                    if( constructorWithCapacity != null )
                    {
                        var getCountMethod = context.SourcePropertyType.GetProperty( "Count" ).GetGetMethod();
                        newInstanceExp = Expression.New( constructorWithCapacity, Expression.Call( context.SourcePropertyVar, getCountMethod ) );
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

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();
            var newElement = Expression.Variable( context.TargetElementType, "newElement" );

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod != null || context.Mapping.TypeMapping.GlobalConfiguration
                .ReferenceMappingStrategy == ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL )
            {
                return Expression.Block
                (
                    new[] { newElement },

                    Expression.Assign( context.TargetPropertyVar, GetTargetInstanceExpression() ),
                    ExpressionLoops.ForEach( context.SourcePropertyVar, context.SourceLoopingVar, Expression.Block
                    (
                        Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                        Expression.Call( context.TargetPropertyVar, addMethod, newElement ),

                        Expression.Call( context.ReturnObjectVar, addToRefCollectionMethod,
                            Expression.New( objectPairConstructor, context.SourceLoopingVar, newElement ) )
                    ) )
                );
            }

            //Look for the constructor which takes an initial collection as parameter
            var constructor = GetTargetCollectionConstructorFromCollection( context );
            if( constructor == null )
            {
                string msg = $@"'{nameof( context.TargetPropertyType )}' does not provide an 'Add' method or a constructor taking as parameter IEnumerable<T>. " +
                    $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item insertion method.";

                throw new Exception( msg );
            }

            var tempCollectionType = typeof( List<> ).MakeGenericType( context.TargetElementType );
            var tempCollection = Expression.Parameter( tempCollectionType, "tempCollection" );
            var tempCollectionAddMethod = tempCollectionType.GetMethod( "Add" );

            var tempCtorWithCapacity = tempCollectionType.GetConstructor( new Type[] { typeof( int ) } );
            var tempCollectionCountMethod = context.SourcePropertyType.GetProperty( "Count" ).GetGetMethod();

            var newTempCollectionExp = Expression.New( tempCtorWithCapacity,
                Expression.Call( context.SourcePropertyVar, tempCollectionCountMethod ) );

            return Expression.Block
            (
                new[] { newElement, tempCollection },

                Expression.Assign( tempCollection, newTempCollectionExp ),
                ExpressionLoops.ForEach( context.SourcePropertyVar, context.SourceLoopingVar, Expression.Block
                (
                    Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                    Expression.Call( tempCollection, tempCollectionAddMethod, newElement ),

                    Expression.Call( context.ReturnObjectVar, addToRefCollectionMethod,
                        Expression.New( objectPairConstructor, context.SourceLoopingVar, newElement ) )
                ) ),

                Expression.Assign( context.TargetPropertyVar, Expression.New( constructor, tempCollection ) )
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
            return Expression.Assign( context.ReturnObjectVar,
                Expression.New( context.ReturnType ) );
        }

        protected virtual ConstructorInfo GetTargetCollectionConstructorFromCollection( CollectionMapperContext context )
        {
            var paramType = new Type[] { typeof( IEnumerable<> )
                .MakeGenericType( context.TargetElementType ) };

            return context.TargetPropertyType.GetConstructor( paramType );
        }

        /// <summary>
        /// Return the method that allows to add items to the target collection.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetTargetCollectionAddMethod( CollectionMapperContext context )
        {
            return context.TargetPropertyType.GetMethod( "Add" );
        }
    }
}


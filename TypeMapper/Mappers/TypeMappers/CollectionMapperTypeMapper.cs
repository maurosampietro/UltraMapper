using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.TypeMappers
{

    public class CollectionMapperTypeMapping : ReferenceMapperTypeMapping
    {
        public override bool CanHandle( TypeMapping mapping )
        {
            return mapping.TypePair.SourceType.IsEnumerable() &&
                 mapping.TypePair.TargetType.IsEnumerable();
        }

        protected override object GetMapperContext( TypeMapping mapping )
        {
            return new CollectionMapperContextTypeMapping( mapping );
        }

        protected virtual Expression GetSimpleTypeInnerBody( TypeMapping mapping, CollectionMapperContextTypeMapping context )
        {
            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetPropertyType )}' does not provide a clear method";
                throw new Exception( msg );
            }

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod == null )
            {
                string msg = $@"Cannot use existing instance on target object. '{nameof( context.TargetPropertyType )}' does not provide an item-insertion method " +
                    $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item-insertion method.";

                throw new Exception( msg );
            }

            var conversion = MappingExpressionBuilderFactory.GetMappingExpression(
                context.SourceElementType, context.TargetElementType );

            Expression loopBody = Expression.Call( context.TargetInstance,
                addMethod, Expression.Invoke( conversion, context.SourceLoopingVar ) );

            return Expression.Block
            (
                Expression.Call( context.TargetInstance, clearMethod ),
                ExpressionLoops.ForEach( context.SourcePropertyVar,
                    context.SourceLoopingVar, loopBody )
            );
        }

        private MethodInfo GetTargetCollectionClearMethod( CollectionMapperContextTypeMapping context )
        {
            return context.TargetPropertyType.GetMethod( "Clear" );
        }

        protected virtual Expression GetComplexTypeInnerBody( TypeMapping mapping, CollectionMapperContextTypeMapping context )
        {
            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();
            var newElement = Expression.Variable( context.TargetElementType, "newElement" );

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetPropertyType )}' does not provide a clear method";
                throw new Exception( msg );
            }

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod == null )
            {
                string msg = $@"Cannot use existing instance on target object. '{nameof( context.TargetPropertyType )}' does not provide an item-insertion method " +
                      $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item-insertion method.";

                throw new Exception( msg );
            }

            //in case of a struct 
            Expression loopingVarToObject = context.SourceLoopingVar;
            if( context.SourceElementType.IsPrimitive )
                loopingVarToObject = Expression.Convert( context.SourceLoopingVar, typeof( object ) );

            //in case of a struct 
            Expression targetVarToObject = newElement;
            if( context.TargetElementType.IsPrimitive )
                targetVarToObject = Expression.Convert( newElement, typeof( object ) );

            return Expression.Block
            (
                new[] { newElement },

                Expression.Call( context.TargetInstance, clearMethod ),
                ExpressionLoops.ForEach( context.SourcePropertyVar, context.SourceLoopingVar, Expression.Block
                (
                    Expression.Assign( newElement, Expression.New( context.TargetElementType ) ),
                    Expression.Call( context.TargetInstance, addMethod, newElement ),

                    Expression.Call( context.ReturnObjectVar, addToRefCollectionMethod,
                        Expression.New( objectPairConstructor, loopingVarToObject, targetVarToObject ) )
                ) )
            );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as CollectionMapperContextTypeMapping;

            if( context.IsSourceElementTypeBuiltIn && context.IsTargetElementTypeBuiltIn )
                return GetSimpleTypeInnerBody( context.Mapping, context );

            return GetComplexTypeInnerBody( context.Mapping, context );
        }

        protected override Expression ReturnTypeInitialization( object contextObj )
        {
            var context = contextObj as CollectionMapperContextTypeMapping;
            return Expression.Assign( context.ReturnObjectVar,
                Expression.New( context.ReturnType ) );
        }

        /// <summary>
        /// Return the method that allows to add items to the target collection.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual MethodInfo GetTargetCollectionAddMethod( CollectionMapperContextTypeMapping context )
        {
            return context.TargetPropertyType.GetMethod( "Add" );
        }
    }
}

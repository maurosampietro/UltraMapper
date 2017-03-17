using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.TypeMappers
{
    public class CollectionMapperTypeMapping : ReferenceMapper, ITypeMappingMapperExpression
    {
        public CollectionMapperTypeMapping( GlobalConfiguration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsEnumerable() && target.IsEnumerable() &&
                 !source.IsBuiltInType( false ); //avoid strings
        }

        protected override object GetMapperContext( Type source, Type target )
        {
            return new CollectionMapperContext( source, target );
        }

        protected virtual Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetMemberType )}' does not provide a clear method";
                throw new Exception( msg );
            }

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

            Expression loopBody = Expression.Call( context.TargetInstance,
                addMethod, Expression.Invoke( convert, context.SourceCollectionLoopingVar ) );

            return Expression.Block
            (
                Expression.Call( context.TargetInstance, clearMethod ),
                ExpressionLoops.ForEach( context.SourceMember,
                    context.SourceCollectionLoopingVar, loopBody )
            );
        }

        protected virtual Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            var itemMapping = MapperConfiguration.Configurator[
                context.SourceCollectionElementType, context.TargetCollectionElementType ].MappingExpression;

            var newElement = Expression.Variable( context.TargetCollectionElementType, "newElement" );

            var clearMethod = GetTargetCollectionClearMethod( context );
            if( clearMethod == null )
            {
                string msg = $@"Cannot map to type '{nameof( context.TargetMemberType )}' does not provide a clear method";
                throw new Exception( msg );
            }

            var addMethod = GetTargetCollectionAddMethod( context );
            if( addMethod == null )
            {
                string msg = $@"Cannot use existing instance on target object. '{nameof( context.TargetMemberType )}' does not provide an item-insertion method " +
                      $"Please override '{nameof( GetTargetCollectionAddMethod )}' to provide the item-insertion method.";

                throw new Exception( msg );
            }

            return Expression.Block
            (
                new[] { newElement },

                Expression.Call( context.TargetInstance, clearMethod ),
                ExpressionLoops.ForEach( context.SourceMember, context.SourceCollectionLoopingVar, Expression.Block
                (
                    Expression.Assign( newElement, Expression.New( context.TargetCollectionElementType ) ),
                    Expression.Call( context.TargetInstance, addMethod, newElement ),

                    Expression.Invoke( itemMapping, context.ReferenceTrack, context.SourceCollectionLoopingVar, newElement )
                ) )
            );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as CollectionMapperContext;

            if( context.IsSourceElementTypeBuiltIn && context.IsTargetElementTypeBuiltIn )
                return GetSimpleTypeInnerBody( context );

            return GetComplexTypeInnerBody( context );
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

        private MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
        {
            return context.TargetMemberType.GetMethod( "Clear" );
        }
    }
}

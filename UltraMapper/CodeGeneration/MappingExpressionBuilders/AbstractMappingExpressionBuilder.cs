using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class AbstractMappingExpressionBuilder : ReferenceMapper
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return source.EntryType.IsAbstract || source.EntryType.IsInterface || source.EntryType == typeof( object ) ||
                target.EntryType.IsAbstract || target.EntryType.IsInterface || target.EntryType == typeof( object );
        }

        public override LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            var context = GetMapperContext( mapping );

            var typeMapping = context.MapperConfiguration[ source.EntryType, target.EntryType ];

            ////non recursive
            //var expression = Expression.Block
            //(
            //    Expression.Invoke( typeMapping.MappingExpression, context.ReferenceTracker, context.SourceInstance, context.TargetInstance )
            //);

            //recursive 
            var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                .MakeGenericMethod( source.EntryType, target.EntryType );

            var expression = Expression.Block
            (
                new[] { context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( context.MapperInstance ) ),

                Expression.Call( context.Mapper, mapMethod,
                    context.SourceInstance, context.TargetInstance,
                    context.ReferenceTracker, Expression.Constant( typeMapping ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}

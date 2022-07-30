using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ReferenceToStructMapper : ReferenceMapper
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return !source.EntryType.IsBuiltIn( false ) && !target.EntryType.IsBuiltIn( false );
        }

        public override LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var context = this.GetMapperContext( mapping );

            var expression = Expression.Block
            (
                base.GetMappingExpression( mapping ).Body
                    .ReplaceParameter( context.Mapper, context.Mapper.Name )
                    .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                    .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name )
                    .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name ),

                context.TargetInstance
            );

            var delegateType = typeof( UltraMapperFunc<,> )
                .MakeGenericType( context.SourceInstance.Type, context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}

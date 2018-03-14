using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class ReferenceToStructMapper : ReferenceMapper
    {
        public ReferenceToStructMapper( Configuration configuration ) : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return !source.IsBuiltInType( false ) && !target.IsBuiltInType( false );
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = this.GetMapperContext( source, target, options );

            var expression = Expression.Block
            (
                base.GetMappingExpression( source, target, options ).Body
                    .ReplaceParameter( context.Mapper, context.Mapper.Name )
                    .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                    .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name )
                    .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name ),

                context.TargetInstance
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type, context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];
            var memberMappings = this.GetMemberMappings( typeMapping )
                .ReplaceParameter( context.Mapper, context.Mapper.Name )
                .ReplaceParameter( context.ReferenceTracker, context.ReferenceTracker.Name )
                .ReplaceParameter( context.TargetInstance, context.TargetInstance.Name )
                .ReplaceParameter( context.SourceInstance, context.SourceInstance.Name );

            var expression = Expression.Block
            (
                new[] { context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),

                memberMappings,
                this.GetExpressionBody( context ),

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

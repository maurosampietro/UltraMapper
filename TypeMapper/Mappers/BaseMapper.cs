using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class BaseMapper
    {
        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            //Action<sourceType, targetType>

            var context = new MapperContext( mapping );
            var valueAssignment = this.GetValueAssignment( context );

            var targetSetter = mapping.TargetProperty.ValueSetter;
            var targetSetterInstanceParamName = targetSetter.Parameters[ 0 ].Name;
            var targetSetterValueParamName = targetSetter.Parameters[ 1 ].Name;

            var body = (Expression)Expression.Block
            (
                new[] { context.TargetValue },

                valueAssignment
                    .ReplaceParameter( context.SourceInstance ),

                mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( context.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( context.TargetValue, targetSetterValueParamName )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceType, context.TargetType );

            return Expression.Lambda( delegateType, body, context.ReferenceTrack,
                context.SourceInstance, context.TargetInstance );
        }

        protected abstract Expression GetValueAssignment( MapperContext context );
    }
}

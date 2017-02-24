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

            var context = this.GetContext( mapping );
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
                typeof( ReferenceTracking ), context.SourceInstanceType, context.TargetInstanceType );

            return Expression.Lambda( delegateType, body, context.ReferenceTrack,
                context.SourceInstance, context.TargetInstance );
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            var context = this.GetContext( sourceType, targetType );
            var valueAssignment = this.GetValueAssignment( context );

            var body = Expression.Block
            (
                new[] { context.TargetValue },
                
                valueAssignment,

                //return this value
                context.TargetValue
            );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType, body, context.SourceInstance );
        }

        protected virtual MapperContext GetContext( MemberMapping mapping )
        {
            return new MapperContext( mapping );
        }

        protected virtual MapperContext GetContext( Type sourceType, Type targetType )
        {
            return new MapperContext( sourceType, targetType );
        }

        protected abstract Expression GetValueAssignment( MapperContext context );
    }
}

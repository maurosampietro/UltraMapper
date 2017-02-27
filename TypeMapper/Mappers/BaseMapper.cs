using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class BaseMapper
    {
        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            //Action<SourceType, TargetType>

            var context = this.GetContext( mapping );
            var valueAssignment = this.GetTargetValueAssignment( context );

            var targetSetter = mapping.TargetProperty.ValueSetter;
            var targetSetterInstanceParamName = targetSetter.Parameters[ 0 ].Name;
            var targetSetterValueParamName = targetSetter.Parameters[ 1 ].Name;

            var body = (Expression)Expression.Block
            (
                new[] { context.TargetMember },

                valueAssignment
                    .ReplaceParameter( context.SourceInstance ),

                mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( context.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( context.TargetMember, targetSetterValueParamName )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceInstanceType, 
                context.TargetInstanceType );

            return Expression.Lambda( delegateType, body, context.ReferenceTrack,
                context.SourceInstance, context.TargetInstance );
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            //Func<SourceType, TargetType>

            var context = this.GetContext( sourceType, targetType );
            var targetValueAssignment = this.GetTargetValueAssignment( context );

            var body = Expression.Block
            (
                new[] { context.TargetMember },
                
                targetValueAssignment,

                //return the value assigned to TargetValue param
                context.TargetMember
            );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType, 
                body, context.SourceInstance );
        }

        protected virtual MapperContext GetContext( MemberMapping mapping )
        {
            return new MapperContext( mapping );
        }

        protected virtual MapperContext GetContext( Type sourceType, Type targetType )
        {
            return new MapperContext( sourceType, targetType );
        }

        protected abstract Expression GetTargetValueAssignment( MapperContext context );
    }
}

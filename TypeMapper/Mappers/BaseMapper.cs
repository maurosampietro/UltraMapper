using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class BaseMapper : ITypeMappingMapperExpression
    {
        public readonly GlobalConfiguration MapperConfiguration;

        public BaseMapper( GlobalConfiguration configuration )
        {
            this.MapperConfiguration = configuration;
        }

        public abstract bool CanHandle( Type sourceType, Type targetType );

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
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

        protected virtual MapperContext GetContext( Type sourceType, Type targetType )
        {
            return new MapperContext( sourceType, targetType );
        }

        protected abstract Expression GetTargetValueAssignment( MapperContext context );
    }
}

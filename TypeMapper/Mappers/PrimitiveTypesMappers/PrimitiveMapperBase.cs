using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class PrimitiveMapperBase : IMapperExpressionBuilder
    {
        protected readonly MapperConfiguration MapperConfiguration;

        public PrimitiveMapperBase( MapperConfiguration configuration )
        {
            this.MapperConfiguration = configuration;
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            var context = this.GetContext( sourceType, targetType );
            var targetValueAssignment = this.GetTargetValueAssignment( context );

            var body = Expression.Block
            (
                new[] { context.TargetInstance },

                targetValueAssignment,

                //return the value assigned to TargetValue param
                context.TargetInstance
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

        public abstract bool CanHandle( Type sourceType, Type targetType );

        protected abstract Expression GetTargetValueAssignment( MapperContext context );
    }
}

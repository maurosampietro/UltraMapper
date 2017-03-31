using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Mappers
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
            var getValueExpression = this.GetValueExpression( context );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType,
                getValueExpression, context.SourceInstance );
        }

        protected virtual MapperContext GetContext( Type sourceType, Type targetType )
        {
            return new MapperContext( sourceType, targetType );
        }

        public abstract bool CanHandle( Type sourceType, Type targetType );

        protected abstract Expression GetValueExpression( MapperContext context );
    }
}

using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public abstract class PrimitiveMapperBase : IMappingExpressionBuilder
    {
        public LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            var context = this.GetContext( source.EntryType, target.EntryType, mapping );
            var getValueExpression = this.GetValueExpression( context );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( source.EntryType, target.EntryType );

            return Expression.Lambda( delegateType,
                getValueExpression, context.SourceInstance );
        }

        protected virtual MapperContext GetContext( Type sourceType, Type targetType, Mapping mapping )
        {
            return new MapperContext( sourceType, targetType, (IMappingOptions)mapping );
        }

        public abstract bool CanHandle( Mapping mapping );

        protected abstract Expression GetValueExpression( MapperContext context );
    }
}

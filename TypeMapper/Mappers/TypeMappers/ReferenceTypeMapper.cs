using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.TypeMappers
{
    public class ReferenceMapperTypeMapping : ReferenceMapper
    {
        public virtual bool CanHandle( TypeMapping mapping )
        {
            var sourceType = mapping.TypePair.SourceType;
            var targetType = mapping.TypePair.TargetType;

            return this.CanHandle( sourceType, targetType );
        }

        protected virtual object GetMapperContext( TypeMapping mapping )
        {
            return new ReferenceMapperContext( mapping );
        }

        public LambdaExpression GetMappingExpression( TypeMapping mapping )
        {
            var context = this.GetMapperContext( mapping ) as ReferenceMapperContext;
            return this.GetMappingExpression( context );
        }
    }
}

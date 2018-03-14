using System;
using System.Linq.Expressions;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MapperContext
    {
        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public IMappingOptions Options { get; protected set; }

        public MapperContext( Type source, Type target, IMappingOptions options )
        {
            SourceInstance = Expression.Parameter( source, "sourceInstance" );
            TargetInstance = Expression.Parameter( target, "targetInstance" );

            Options = options;
        }
    }
}

using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class MapperContext
    {
        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }

        public MapperContext( Type source, Type target )
        {
            SourceInstance = Expression.Parameter( source, "sourceInstance" );
            TargetInstance = Expression.Parameter( target, "targetInstance" );
        }
    }
}

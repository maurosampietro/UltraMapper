using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Mappers
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

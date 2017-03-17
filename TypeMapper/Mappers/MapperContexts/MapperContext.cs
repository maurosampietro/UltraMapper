using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class MapperContext
    {
        public Type SourceInstanceType { get; protected set; }
        public Type TargetInstanceType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }

        public MapperContext( Type source, Type target )
        {
            SourceInstanceType = source;
            TargetInstanceType = target;

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
        }
    }
}

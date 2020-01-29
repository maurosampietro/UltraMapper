using System;
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    internal interface IMapping
    {
        Action<ReferenceTracking, object, object> MappingFunc { get; }
        LambdaExpression MappingExpression { get; }
    }
}
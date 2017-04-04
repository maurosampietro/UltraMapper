using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public interface IMapping
    {
        Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> MappingFunc { get; }
        LambdaExpression MappingExpression { get; }
    }
}
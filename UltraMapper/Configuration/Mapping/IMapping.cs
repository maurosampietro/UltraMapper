using System;
using System.Linq.Expressions;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public interface IMapping
    {
        IMappingSource Source { get; }
        IMappingTarget Target { get; }
        IMappingExpressionBuilder Mapper { get; }
        UltraMapperDelegate MappingFunc { get; }
        LambdaExpression MappingExpression { get; }
    }
}
﻿using System;
using System.Linq.Expressions;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public interface IMapping
    {
        IMappingExpressionBuilder Mapper { get; }
        Action<ReferenceTracker, object, object> MappingFunc { get; }
        LambdaExpression MappingExpression { get; }
    }
}
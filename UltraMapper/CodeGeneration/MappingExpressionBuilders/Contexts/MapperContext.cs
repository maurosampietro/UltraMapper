﻿using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MapperContext
    {
        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public IMappingOptions Options { get; protected set; }
        public Mapping Mapping { get; }
        public Configuration MapperConfiguration { get; }

        public MapperContext( Type source, Type target, IMappingOptions options )
        {
            this.SourceInstance = Expression.Parameter( source, "sourceInstance" );
            this.TargetInstance = Expression.Parameter( target, "targetInstance" );
            this.Options = options;
            this.Mapping = (Mapping)options;
            this.MapperConfiguration = this.Mapping.GlobalConfig;
        }
    }
}

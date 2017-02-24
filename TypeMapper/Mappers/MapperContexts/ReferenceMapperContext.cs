﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapperContext
    {
        public readonly MemberMapping Mapping;

        public Type ReturnType { get; protected set; }
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }

        public Type SourceType { get; protected set; }
        public Type TargetType { get; protected set; }
        public Type SourcePropertyType { get; protected set; }
        public Type TargetPropertyType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetPropertyVar { get; protected set; }
        public ParameterExpression SourcePropertyVar { get; protected set; }
        public ParameterExpression ReturnObjectVar { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ReferenceMapperContext( MemberMapping mapping )
        {
            Mapping = mapping;

            ReturnType = typeof( ObjectPair );
            ReturnTypeConstructor = ReturnType.GetConstructors().First();

            SourceType = mapping.TypeMapping.TypePair.SourceType;
            TargetType = mapping.TypeMapping.TypePair.TargetType;

            SourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            TargetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( SourceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourcePropertyVar = Expression.Variable( SourcePropertyType, "sourceArg" );
            TargetPropertyVar = Expression.Variable( TargetPropertyType, "targetArg" );
            ReturnObjectVar = Expression.Variable( ReturnType, "result" );

            SourceNullValue = Expression.Constant( null, SourcePropertyType );
            TargetNullValue = Expression.Constant( null, TargetPropertyType );
        }
    }

    public class ReferenceMapperContextTypeMapping
    {
        public readonly TypeMapping Mapping;

        public Type ReturnType { get; protected set; }
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }

        public Type SourceType { get; protected set; }
        public Type TargetType { get; protected set; }
        public Type SourcePropertyType { get; protected set; }
        public Type TargetPropertyType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetPropertyVar { get; protected set; }
        public ParameterExpression SourcePropertyVar { get; protected set; }
        public ParameterExpression ReturnObjectVar { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ReferenceMapperContextTypeMapping( TypeMapping mapping )
        {
            Mapping = mapping;

            ReturnType = typeof( ObjectPair );
            ReturnTypeConstructor = ReturnType.GetConstructors().First();

            SourceType = mapping.TypePair.SourceType;
            TargetType = mapping.TypePair.TargetType;

            SourcePropertyType = mapping.TypePair.SourceType;
            TargetPropertyType = mapping.TypePair.TargetType;

            SourceInstance = Expression.Parameter( SourceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourcePropertyVar = Expression.Variable( SourcePropertyType, "sourceArg" );
            TargetPropertyVar = Expression.Variable( TargetPropertyType, "targetArg" );
            ReturnObjectVar = Expression.Variable( ReturnType, "result" );

            SourceNullValue = Expression.Constant( null, SourcePropertyType );
            TargetNullValue = Expression.Constant( null, TargetPropertyType );
        }
    }
}
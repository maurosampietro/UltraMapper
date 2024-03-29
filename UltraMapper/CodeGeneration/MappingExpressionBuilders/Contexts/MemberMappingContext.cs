﻿using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MemberMappingContext : ReferenceMapperContext
    {
        public ParameterExpression TrackedReference { get; protected set; }

        public ParameterExpression TargetMember { get; protected set; }
        public ParameterExpression SourceMember { get; protected set; }

        public Expression SourceMemberValueGetter { get; protected set; }
        public Expression TargetMemberValueGetter { get; protected set; }

        public Expression TargetMemberValueSetter { get; protected set; }

        public Expression TargetMemberNullValue { get; internal set; }
        public Expression SourceMemberNullValue { get; internal set; }

        public readonly MemberMapping MemberMapping;

        public MemberMappingContext( MemberMapping mapping )
            : base( mapping.InstanceTypeMapping.Source.EntryType,
                    mapping.InstanceTypeMapping.Target.EntryType, mapping )
        {
            this.MemberMapping = mapping;

            var sourceMemberType = mapping.SourceMember.MemberType;
            var targetMemberType = mapping.TargetMember.MemberType;

            TrackedReference = Expression.Parameter( targetMemberType, "trackedReference" );

            SourceMember = Expression.Variable( sourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( targetMemberType, "targetValue" );

            if( !sourceMemberType.IsValueType )
                SourceMemberNullValue = Expression.Constant( null, sourceMemberType );

            if( !targetMemberType.IsValueType )
                TargetMemberNullValue = Expression.Constant( null, targetMemberType );

            var sourceGetterInstanceParamName = mapping.SourceMember
                .ValueGetter.Parameters[ 0 ].Name;

            SourceMemberValueGetter = mapping.SourceMember.ValueGetter.Body
                .ReplaceParameter( SourceInstance, sourceGetterInstanceParamName );

            if( mapping.TargetMember.ValueGetter != null )
            {
                var targetGetterInstanceParamName = mapping.TargetMember
                    .ValueGetter.Parameters[ 0 ].Name;

                TargetMemberValueGetter = mapping.TargetMember.ValueGetter.Body
                    .ReplaceParameter( TargetInstance, targetGetterInstanceParamName );
            }

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            TargetMemberValueSetter = mapping.TargetMember.ValueSetter.Body
                .ReplaceParameter( TargetInstance, targetSetterInstanceParamName )
                .ReplaceParameter( TargetMember, targetSetterMemberParamName );
        }
    }
}

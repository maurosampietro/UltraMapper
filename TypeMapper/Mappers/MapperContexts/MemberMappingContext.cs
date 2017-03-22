using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.MapperContexts
{
    public class MemberMappingContext : MapperContext
    {
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetMember { get; protected set; }
        public ParameterExpression SourceMember { get; protected set; }

        public Expression SourceMemberValue { get; protected set; }
        public Expression TargetMemberValue { get; protected set; }

        public Expression TargetMemberValueSetter { get; protected set; }
        public Expression TargetMemberNullValue { get; internal set; }
        public Expression SourceMemberNullValue { get; internal set; }

        public MemberMappingContext( MemberMapping mapping )
            : base( mapping.InstanceTypeMapping.TypePair.SourceType, mapping.InstanceTypeMapping.TypePair.TargetType )
        {
            var sourceInstanceType = mapping.InstanceTypeMapping.TypePair.SourceType;
            var targetInstanceType = mapping.InstanceTypeMapping.TypePair.TargetType;

            var sourceMemberType = mapping.SourceMember.MemberInfo.GetMemberType();
            var targetMemberType = mapping.TargetMember.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( sourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( targetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceMember = Expression.Variable( sourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( targetMemberType, "targetValue" );

            SourceMemberNullValue = Expression.Constant( null, sourceMemberType );
            TargetMemberNullValue = Expression.Constant( null, targetMemberType );

            var sourceGetterInstanceParamName = mapping.SourceMember
                .ValueGetter.Parameters[ 0 ].Name;

            SourceMemberValue = mapping.SourceMember.ValueGetter.Body
                .ReplaceParameter( SourceInstance, sourceGetterInstanceParamName );

            var targetGetterInstanceParamName = mapping.TargetMember
                .ValueGetter.Parameters[ 0 ].Name;

            TargetMemberValue = mapping.TargetMember.ValueGetter.Body
                .ReplaceParameter( TargetInstance, targetGetterInstanceParamName );

            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

            TargetMemberValueSetter = mapping.TargetMember.ValueSetter.Body
                .ReplaceParameter( TargetInstance, targetSetterInstanceParamName )
                .ReplaceParameter( TargetMember, targetSetterMemberParamName );
        }
    }
}

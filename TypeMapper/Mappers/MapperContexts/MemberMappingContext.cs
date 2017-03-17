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
        public Type SourceMemberType { get; protected set; }
        public Type TargetMemberType { get; protected set; }

        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetMember { get; protected set; }
        public ParameterExpression SourceMember { get; protected set; }

        public Expression SourceMemberValue { get; protected set; }
        public Expression TargetMemberValue { get; protected set; }

        public Expression TargetMemberValueSetter { get; protected set; }

        public MemberMappingContext( MemberMapping mapping )
            : this( mapping.MemberTypeMapping )
        {
            SourceInstanceType = mapping.InstanceTypeMapping.TypePair.SourceType;
            TargetInstanceType = mapping.InstanceTypeMapping.TypePair.TargetType;

            SourceMemberType = mapping.SourceMember.MemberInfo.GetMemberType();
            TargetMemberType = mapping.TargetMember.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceMember = Expression.Variable( SourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( TargetMemberType, "targetValue" );

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

        public MemberMappingContext( TypeMapping mapping )
            : this( mapping.TypePair.SourceType, mapping.TypePair.TargetType ) { }

        public MemberMappingContext( Type source, Type target )
            : base( source, target )
        {
            SourceInstanceType = source;
            TargetInstanceType = target;

            SourceMemberType = source;
            TargetMemberType = target;

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceMember = Expression.Variable( SourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( TargetMemberType, "targetValue" );

            SourceMemberValue = SourceInstance;
        }
    }
}

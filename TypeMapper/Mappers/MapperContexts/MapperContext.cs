using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class MapperContext
    {
        public readonly GlobalConfiguration MapperConfiguration;

        public Type SourceInstanceType { get; protected set; }
        public Type TargetInstanceType { get; protected set; }

        public Type SourceMemberType { get; protected set; }
        public Type TargetMemberType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetMember { get; protected set; }
        public ParameterExpression SourceMember { get; protected set; }

        public Expression SourceMemberValue { get; protected set; }
        public Expression TargetMemberValue { get; protected set; }

        public Expression TargetMemberValueSetter { get; protected set; }

        public MapperContext( MemberMapping mapping )
        {
            MapperConfiguration = mapping.InstanceTypeMapping.GlobalConfiguration;

            SourceInstanceType = mapping.InstanceTypeMapping.TypePair.SourceType;
            TargetInstanceType = mapping.InstanceTypeMapping.TypePair.TargetType;

            SourceMemberType = mapping.SourceProperty.MemberInfo.GetMemberType();
            TargetMemberType = mapping.TargetProperty.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceMember = Expression.Variable( SourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( TargetMemberType, "targetValue" );

            var sourceGetterInstanceParamName = mapping.SourceProperty
                .ValueGetter.Parameters[ 0 ].Name;

            SourceMemberValue = mapping.SourceProperty.ValueGetter.Body
                .ReplaceParameter( SourceInstance, sourceGetterInstanceParamName );

            var targetGetterInstanceParamName = mapping.TargetProperty
                .ValueGetter.Parameters[ 0 ].Name;

            TargetMemberValue = mapping.TargetProperty.ValueGetter.Body
                .ReplaceParameter( TargetInstance, targetGetterInstanceParamName );

            var targetSetterInstanceParamName = mapping.TargetProperty.ValueSetter.Parameters[ 0 ].Name;
            var targetSetterMemberParamName = mapping.TargetProperty.ValueSetter.Parameters[ 1 ].Name;

            TargetMemberValueSetter = mapping.TargetProperty.ValueSetter.Body
                .ReplaceParameter( TargetInstance, targetSetterInstanceParamName )
                .ReplaceParameter( TargetMember, targetSetterMemberParamName );
        }

        public MapperContext( Type source, Type target )
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

        public MapperContext( TypeMapping mapping )
            : this( mapping.TypePair.SourceType, mapping.TypePair.TargetType )
        {
            this.MapperConfiguration = mapping.GlobalConfiguration;
        }
    }
}

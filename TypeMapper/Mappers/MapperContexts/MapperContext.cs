using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class MapperContext
    {
        public readonly MemberMapping Mapping;

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

        public MapperContext( MemberMapping mapping )
        {
            Mapping = mapping;

            SourceInstanceType = mapping.TypeMapping.TypePair.SourceType;
            TargetInstanceType = mapping.TypeMapping.TypePair.TargetType;

            SourceMemberType = mapping.SourceProperty.MemberInfo.GetMemberType();
            TargetMemberType = mapping.TargetProperty.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceMember = Expression.Variable( SourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( TargetMemberType, "targetValue" );

            var sourceGetterInstanceParamName = Mapping.SourceProperty
                .ValueGetter.Parameters[ 0 ].Name;

            SourceMemberValue = Mapping.SourceProperty.ValueGetter.Body
                .ReplaceParameter( SourceInstance, sourceGetterInstanceParamName );

            var targetGetterInstanceParamName = Mapping.TargetProperty
                .ValueGetter.Parameters[ 0 ].Name;

            TargetMemberValue = Mapping.TargetProperty.ValueGetter.Body
                .ReplaceParameter( TargetInstance, targetGetterInstanceParamName );
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

            TargetMember = Expression.Variable( TargetMemberType, "targetValue" );
            SourceMemberValue = SourceInstance;
        }
    }
}

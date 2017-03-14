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

        //public ITypeOptions InstanceMappingOptions { get; protected set; }
        //public ITypeOptions MemberMappingOptions { get; protected set; }

        public MapperContext( MemberMapping mapping )
        {
            MapperConfiguration = mapping.InstanceTypeMapping.GlobalConfiguration;

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

            //InstanceMappingOptions = mapping.InstanceTypeMapping.GlobalConfiguration
            //    .Configurator[ SourceInstance.Type, TargetInstance.Type ];

            //MemberMappingOptions = mapping as ITypeOptions;
        }

        public MapperContext( TypeMapping mapping )
        {
            MapperConfiguration = mapping.GlobalConfiguration;

            SourceInstanceType = mapping.TypePair.SourceType;
            TargetInstanceType = mapping.TypePair.TargetType;

            SourceMemberType = mapping.TypePair.SourceType;
            TargetMemberType = mapping.TypePair.TargetType;

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceMember = Expression.Variable( SourceMemberType, "sourceValue" );
            TargetMember = Expression.Variable( TargetMemberType, "targetValue" );

            SourceMemberValue = SourceInstance;

            //InstanceMappingOptions = mapping.GlobalConfiguration.Configurator[
            //    SourceInstance.Type, TargetInstance.Type ];

            //MemberMappingOptions = mapping.GlobalConfiguration.Configurator[
            //    SourceMember.Type, TargetMember.Type ];
        }
    }
}

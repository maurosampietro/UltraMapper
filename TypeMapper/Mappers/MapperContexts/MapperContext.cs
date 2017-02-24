using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class MapperContext
    {
        protected readonly MemberMapping Mapping;

        public Type SourceInstanceType { get; protected set; }
        public Type TargetInstanceType { get; protected set; }

        public Type SourceValueType { get; protected set; }
        public Type TargetValueType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetValue { get; protected set; }
        public Expression SourceValue { get; protected set; }

        public MapperContext( MemberMapping mapping )
        {
            Mapping = mapping;

            SourceInstanceType = mapping.TypeMapping.TypePair.SourceType;
            TargetInstanceType = mapping.TypeMapping.TypePair.TargetType;

            SourceValueType = mapping.SourceProperty.MemberInfo.GetMemberType();
            TargetValueType = mapping.TargetProperty.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            TargetValue = Expression.Variable( TargetValueType, "targetValue" );

            var sourceGetterInstanceParamName = Mapping.SourceProperty
                .ValueGetter.Parameters[ 0 ].Name;

            SourceValue = Mapping.SourceProperty.ValueGetter.Body
                .ReplaceParameter( SourceInstance, sourceGetterInstanceParamName );
        }

        public MapperContext( Type source, Type target )
        {
            SourceInstanceType = source;
            TargetInstanceType = target;

            SourceValueType = source;
            TargetValueType = target;

            SourceInstance = Expression.Parameter( SourceInstanceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetInstanceType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            TargetValue = Expression.Variable( TargetValueType, "targetValue" );
            SourceValue = SourceInstance;
        }
    }
}

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
        public readonly MemberMapping Mapping;

        public Type SourceType { get; protected set; }
        public Type TargetType { get; protected set; }

        public Type SourcePropertyType { get; protected set; }
        public Type TargetPropertyType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetValue { get; protected set; }
        public ParameterExpression SourceValue { get; protected set; }

        public MapperContext( MemberMapping mapping )
        {
            Mapping = mapping;

            SourceType = mapping.TypeMapping.TypePair.SourceType;
            TargetType = mapping.TypeMapping.TypePair.TargetType;

            SourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            TargetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            SourceInstance = Expression.Parameter( SourceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourceValue = Expression.Variable( SourcePropertyType, "sourceArg" );
            TargetValue = Expression.Variable( TargetPropertyType, "targetArg" );
        }
    }
}

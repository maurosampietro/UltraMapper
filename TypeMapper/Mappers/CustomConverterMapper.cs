using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class CustomConverterMapper : IObjectMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            return mapping.CustomConverter != null;
        }

        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            //Action<ReferenceTracking, sourceType, targetType>

            var sourceType = mapping.TypeMapping.TypePair.SourceType;
            var targetType = mapping.TypeMapping.TypePair.TargetType;

            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var value = Expression.Variable( targetPropertyType, "value" );

            var readValueExp = mapping.SourceProperty.ValueGetter.Body
                .ReplaceParameter( sourceInstance, mapping.SourceProperty.ValueGetter.Parameters[ 0 ].Name );

            Expression valueAssignment = Expression.Assign( value,
                Expression.Invoke( mapping.CustomConverter, readValueExp ) );

            var temp = mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( targetInstance, mapping.TargetProperty.ValueSetter.Parameters[ 0 ].Name )
                    .ReplaceParameter( value, mapping.TargetProperty.ValueSetter.Parameters[ 1 ].Name );

            var setValueExp = (Expression)Expression.Block
            (
                new[] { value },

                valueAssignment.ReplaceParameter( sourceInstance ),
                temp
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType );

            return Expression.Lambda( delegateType, setValueExp, 
                referenceTrack, sourceInstance, targetInstance );
        }
    }
}

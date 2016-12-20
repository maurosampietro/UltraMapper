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
        public bool CanHandle( PropertyMapping mapping )
        {
            return mapping.CustomConverter != null;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Action<ReferenceTracking, sourceType, targetType>

            var sourceType = mapping.SourceProperty.PropertyInfo.ReflectedType;
            var targetType = mapping.TargetProperty.PropertyInfo.ReflectedType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var value = Expression.Variable( targetPropertyType, "value" );

            var readValueExp = mapping.SourceProperty.ValueGetter.Body;
            Expression valueAssignment = Expression.Invoke( mapping.CustomConverter, readValueExp );

            var setValueExp = (Expression)Expression.Block
            (
                new[] { value },

                valueAssignment.ReplaceParameter( sourceInstance ),
                mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( targetInstance, "target" )
                    .ReplaceParameter( value, "value" )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType );

            return Expression.Lambda( delegateType,
                setValueExp, referenceTrack, sourceInstance, targetInstance );
        }
    }
}

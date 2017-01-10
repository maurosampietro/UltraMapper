using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public sealed class BuiltInTypeMapper : IObjectMapperExpression
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            bool areTypesBuiltIn = mapping.SourceProperty.IsBuiltInType &&
                mapping.TargetProperty.IsBuiltInType;

            return (areTypesBuiltIn) && (sourcePropertyType == targetPropertyType ||
                    sourcePropertyType.IsImplicitlyConvertibleTo( targetPropertyType ) ||
                    sourcePropertyType.IsExplicitlyConvertibleTo( targetPropertyType ));
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

            Func<Expression> getValueAssignmentExp = () =>
            {
                var readValueExp = mapping.SourceProperty.ValueGetter.Body;

                if( sourcePropertyType == targetPropertyType )
                    return Expression.Assign( value, readValueExp );

                return Expression.Assign( value, Expression.Convert(
                    readValueExp, targetPropertyType ) );

                throw new Exception( $"Cannot handle {mapping}" );
            };

            Expression valueAssignment = getValueAssignmentExp();

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

            return Expression.Lambda( delegateType, setValueExp, 
                referenceTrack, sourceInstance, targetInstance );
        }
    }
}

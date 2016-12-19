using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class BuiltInTypeMapper : IObjectMapperExpression
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            return mapping.SourceProperty.IsBuiltInType &&
                mapping.TargetProperty.IsBuiltInType;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Action<ReferenceTracking, sourceType, targetType>

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var value = Expression.Variable( targetPropertyType, "value" );

            Func<Expression> getValueAssignmentExp = () =>
            {
                var readValueExp = mapping.SourceProperty.ValueGetter.Body;

                if( mapping.CustomConverter != null )
                    return Expression.Invoke( mapping.CustomConverter, readValueExp );

                if( sourcePropertyType == targetPropertyType )
                    return Expression.Assign( value, readValueExp );

                if( sourcePropertyType.IsImplicitlyConvertibleTo( targetPropertyType ) ||
                    sourcePropertyType.IsExplicitlyConvertibleTo( targetPropertyType ) )
                {
                    return Expression.Assign( value, Expression.Convert(
                        readValueExp, targetPropertyType ) );
                }

                var convertMethod = typeof( Convert ).GetMethod( $"To{targetPropertyType.Name}", new[] { sourcePropertyType } );
                return Expression.Call( convertMethod, readValueExp );


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

            return Expression.Lambda( delegateType,
                setValueExp, referenceTrack, sourceInstance, targetInstance );
        }
    }

    //    public class BuiltInTypeMapper : IObjectMapper
    //    {
    //        public bool CanHandle( PropertyMapping mapping )
    //        {
    //            return mapping.TargetProperty.IsBuiltInType;
    //        }

    //        public IEnumerable<ObjectPair> Map( object value, object targetInstance,
    //            PropertyMapping mapping, ReferenceTracking referenceTracker )
    //        {
    //            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
    //            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

    //            if( sourcePropertyType != targetPropertyType )
    //            {
    //                //Convert.ChangeType does not handle conversion to nullable types
    //                var conversionType = targetPropertyType;
    //                if( mapping.TargetProperty.NullableUnderlyingType != null )
    //                    conversionType = mapping.TargetProperty.NullableUnderlyingType;

    //                try
    //                {
    //                    if( value == null && conversionType.IsValueType )
    //                        value = conversionType.GetDefaultValue();
    //                    else
    //                        value = Convert.ChangeType( value, conversionType );
    //                }
    //                catch( Exception ex )
    //                {
    //                    string errorMsg = $"Cannot automatically convert value for the mapping '{mapping}'. Please provide a converter.";
    //                    throw new Exception( errorMsg, ex );
    //                }
    //            }

    //            //mapping.TargetProperty.ValueSetter( targetInstance, value );
    //            yield break;
    //        }
    //    }
}

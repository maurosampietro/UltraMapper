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
            return mapping.TargetProperty.IsBuiltInType ||

                (mapping.TargetProperty.IsNullable &&
                mapping.TargetProperty.NullableUnderlyingType.IsBuiltInType( true ));
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
            Expression getValueExp = mapping.SourceProperty.ValueGetter.Body;
            Expression valueExp = null;

            if( sourcePropertyType == targetPropertyType )
                valueExp = Expression.Assign( value, getValueExp );
            else
            {
                if( mapping.ValueConverterExp != null )
                    valueExp = Expression.Assign( value, Expression.Invoke( mapping.ValueConverterExp, getValueExp ) );
                else
                {
                    if( sourcePropertyType.IsImplicitlyConvertibleTo( targetPropertyType ) ||
                        sourcePropertyType.IsExplicitlyConvertibleTo( targetPropertyType ) )
                    {
                        if( !mapping.SourceProperty.IsNullable && !mapping.TargetProperty.IsNullable )
                            valueExp = Expression.Convert( getValueExp, targetPropertyType );
                        else
                        {
                            valueExp = Expression.Block
                            (
                                Expression.IfThenElse
                                (
                                    Expression.Equal( getValueExp, Expression.Constant( null, sourcePropertyType ) ),
                                    Expression.Assign( value, Expression.Default( targetPropertyType ) ),
                                    Expression.Assign( value, Expression.MakeMemberAccess( getValueExp, sourcePropertyType.GetProperty( "Value" ) ) )
                                )
                            );
                        }
                    }
                }
            }

            var setValueExp = (Expression)Expression.Block
            (
                new[] { value },

                valueExp.ReplaceParameter( sourceInstance ),
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

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
            return mapping.TargetProperty.IsBuiltInType;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>
            var returnType = typeof( List<ObjectPair> );

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            Expression valueExp = Expression.Invoke( mapping.SourceProperty.ValueGetterExpr, sourceInstance );
            if( mapping.ValueConverterExp != null )
                valueExp = Expression.Invoke( mapping.ValueConverterExp, valueExp );
            else
            {
                if( sourcePropertyType.IsImplicitlyConvertibleTo( targetPropertyType ) ||
                    sourcePropertyType.IsExplicitlyConvertibleTo( targetPropertyType ) )
                {
                    valueExp = Expression.Convert( valueExp, targetPropertyType );
                }
            }

            var setValueExp = Expression.Block
            (
                Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance, valueExp ),
                Expression.Constant(null, returnType)
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, returnType );

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

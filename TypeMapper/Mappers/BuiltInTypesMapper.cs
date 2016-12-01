using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class BuiltInTypeMapper : IObjectMapper
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            return mapping.TargetProperty.IsBuiltInType;
        }

        public IEnumerable<ObjectPair> Map( object value, object targetInstance,
            PropertyMapping mapping, IReferenceTracking referenceTracker )
        {
            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            if( sourcePropertyType != targetPropertyType )
            {
                //Convert.ChangeType does not handle conversion to nullable types
                var conversionType = targetPropertyType;
                if( mapping.TargetProperty.NullableUnderlyingType != null )
                    conversionType = mapping.TargetProperty.NullableUnderlyingType;

                try
                {
                    if( value == null && conversionType.IsValueType )
                        value = conversionType.GetDefaultValue();
                    else
                        value = Convert.ChangeType( value, conversionType );
                }
                catch( Exception ex )
                {
                    string errorMsg = $"Cannot automatically convert value for the mapping '{mapping}'. Please provide a converter.";
                    throw new Exception( errorMsg, ex );
                }
            }

            //mapping.TargetProperty.ValueSetter( targetInstance, value );
            yield break;
        }
    }
}

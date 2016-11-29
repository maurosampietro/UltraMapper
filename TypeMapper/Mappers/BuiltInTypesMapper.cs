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
                    // TODO: display generic arguments instead (for example: Nullable<int> instead of Nullable'1)

                    string errorMsg = $"Cannot automatically convert value from '{sourcePropertyType.Name}' to '{targetPropertyType.Name}'. " +
                        $"Please provide a converter for mapping '{mapping.SourceProperty.PropertyInfo.Name} -> {mapping.TargetProperty.PropertyInfo.Name}'";

                    throw new Exception( errorMsg, ex );
                }
            }

            mapping.TargetProperty.ValueSetter( targetInstance, value );
            yield break;
        }
    }
}

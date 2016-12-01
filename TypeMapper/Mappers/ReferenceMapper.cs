using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : IObjectMapper
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            bool valueTypes = !mapping.SourceProperty.PropertyInfo.PropertyType.IsValueType &&
                !mapping.TargetProperty.PropertyInfo.PropertyType.IsValueType;

            return valueTypes && !mapping.TargetProperty.IsBuiltInType &&
                !mapping.SourceProperty.IsBuiltInType && !mapping.SourceProperty.IsEnumerable;
        }

        public IEnumerable<ObjectPair> Map( object value, object targetInstance,
            PropertyMapping mapping, IReferenceTracking referenceTracking )
        {
            var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            if( value == null )
            {
                //mapping.TargetProperty.ValueSetter( targetInstance, null );
                yield break;
            }

            object targetValue = null;
            if( !referenceTracking.TryGetValue( value,
                targetPropertyType, out targetValue ) )
            {
                targetValue = InstanceFactory.CreateObject( targetPropertyType );

                //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                referenceTracking.Add( value, targetPropertyType, targetValue );
                yield return new ObjectPair( value, targetValue );
            }

            //mapping.TargetProperty.ValueSetter( targetInstance, targetValue );
            yield break;
        }
    }
}

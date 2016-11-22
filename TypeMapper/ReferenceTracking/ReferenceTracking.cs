using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    /// <summary>
    /// A reference value type can be mapped to many different types (one instance for each target type).
    /// This class helps tracking and retrieving each reference to the mapped type instance.
    /// </summary>
    public class ReferenceTracking : IReferenceTracking
    {
        private Dictionary<object, Dictionary<Type, object>> _mappings
            = new Dictionary<object, Dictionary<Type, object>>();

        public void Add( object sourceInstance, Type targetType, object targetInstance )
        {
            Dictionary<Type, object> targetInstances;
            if( !_mappings.TryGetValue( sourceInstance, out targetInstances ) )
            {
                targetInstances = new Dictionary<Type, object>();
                _mappings.Add( sourceInstance, targetInstances );
            }

            targetInstances.Add( targetType, targetInstance );
        }

        public bool Contains( object sourceInstance, Type targetType )
        {
            object targetInstance;
            return this.TryGetValue( sourceInstance, targetType, out targetInstance );
        }

        public bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance )
        {
            Dictionary<Type, object> targetInstances;
            if( !_mappings.TryGetValue( sourceInstance, out targetInstances ) )
            {
                targetInstance = null;
                return false;
            }

            return targetInstances.TryGetValue( targetType, out targetInstance );
        }

        public object this[ object sourceInstance, Type targetType ]
        {
            get { return _mappings[ sourceInstance ][ targetType ]; }
        }
    }
}

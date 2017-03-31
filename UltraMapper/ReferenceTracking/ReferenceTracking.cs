using System;
using System.Collections.Generic;

namespace UltraMapper
{
    /// <summary>
    /// This class helps tracking and retrieving each source reference to its mapped target instance.
    /// A reference value type can be mapped to many different types (one instance for each target type).
    /// </summary>
    public class ReferenceTracking
    {
        private Dictionary<int, object> _mappings
            = new Dictionary<int, object>( 8 );

        public void Add( object sourceInstance, Type targetType, object targetInstance )
        {
            var key = this.GetKey( sourceInstance, targetType );

            if( !_mappings.ContainsKey( key ) )
                _mappings.Add( key, targetInstance );
        }

        public bool Contains( object sourceInstance, Type targetType )
        {
            var key = this.GetKey( sourceInstance, targetType );
            return _mappings.ContainsKey( key );
        }

        public bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance )
        {
            var key = this.GetKey( sourceInstance, targetType );
            return _mappings.TryGetValue( key, out targetInstance );
        }

        public object this[ object sourceInstance, Type targetType ]
        {
            get
            {
                var key = this.GetKey( sourceInstance, targetType );
                return _mappings[ key ];
            }
        }

        private int GetKey( object sourceInstance, Type targetType )
        {
            return sourceInstance.GetHashCode() ^ targetType.GetHashCode();
        }
    }
}

using System;
using System.Collections.Generic;

namespace TypeMapper
{
    /// <summary>
    /// This class helps tracking and retrieving each reference to the mapped type instance.
    /// A reference value type can be mapped to many different types (one instance for each target type).
    /// </summary>
    public class ReferenceTracking
    {
        private struct Key
        {
            object Object;
            Type Type;

            public Key( object obj, Type type )
            {
                Object = obj;
                Type = type;
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode();
            }

            public override bool Equals( object obj )
            {
                var key = (Key)obj;

                return Type == key.Type &&
                    Object.ReferenceEquals( key.Object, Object );
            }
        }

        private Dictionary<Key, object> _mappings
            = new Dictionary<Key, object>( 8 );

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

        private Key GetKey( object sourceInstance, Type targetType )
        {
            return new Key( sourceInstance, targetType );
            //return sourceInstance.GetHashCode() ^ targetType.GetHashCode();
        }
    }
}

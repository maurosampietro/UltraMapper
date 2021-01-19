using System;
using System.Collections.Generic;

namespace UltraMapper
{
    /// <summary>
    /// This class helps tracking and retrieving each source reference to its mapped target instance.
    /// A reference type can be mapped to many different types (one instance for each target type).
    /// </summary>
    public class ReferenceTracker
    {
        private struct Key
        {
            public readonly object Instance;
            public readonly Type TargetType;

            public Key( object instance, Type targetType )
            {
                this.Instance = instance;
                this.TargetType = targetType;
            }

            public override int GetHashCode()
            {
                int instanceHashCode = this.Instance?.GetHashCode() ?? 0;
                return instanceHashCode ^ this.TargetType.GetHashCode();
            }

            public override bool Equals( object obj )
            {
                var otherKey = (Key)obj;
                return Object.ReferenceEquals( this.Instance, otherKey.Instance )
                    && this.TargetType == otherKey.TargetType;
            }
        }

        private readonly Dictionary<Key, object> _mappings
            = new Dictionary<Key, object>( 512 );

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
        }
    }
}

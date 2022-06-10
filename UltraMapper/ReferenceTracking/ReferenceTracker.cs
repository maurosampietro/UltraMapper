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

        //<sourceInstance, <targetType,targetInstance>>
        private readonly Dictionary<object, Dictionary<Type, object>> _mappings
            = new Dictionary<object, Dictionary<Type, object>>( 64 );

        public void Add( object sourceInstance, Type targetType, object targetInstance )
        {
            if( !_mappings.ContainsKey( sourceInstance ) )
            {
                var dict = new Dictionary<Type, object>() { { targetType, targetInstance } };
                _mappings.Add( sourceInstance, dict );
            }
            else
            {
                var dict = _mappings[ sourceInstance ];
                if( !dict.ContainsKey( targetType ) )
                    dict.Add( targetType, targetInstance );
            }
        }

        public bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance )
        {
            if( _mappings.TryGetValue( sourceInstance, out Dictionary<Type, object> dict ) )
                return dict.TryGetValue( targetType, out targetInstance );

            targetInstance = null;
            return false;
        }

        public bool Contains( object sourceInstance, Type targetType )
        {
            if( _mappings.ContainsKey( sourceInstance ) )
                return _mappings[ sourceInstance ].ContainsKey( targetType );

            return false;
        }

        public object this[ object sourceInstance, Type targetType ]
            => _mappings[ sourceInstance ][ targetType ];
    }
}
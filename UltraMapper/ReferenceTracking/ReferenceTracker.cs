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
        //<sourceInstance, <targetType,targetInstance>>
        private readonly Dictionary<object, Dictionary<Type, object>> _mappings
            = new Dictionary<object, Dictionary<Type, object>>( 64 );

        public void Add( object sourceInstance, Type targetType, object targetInstance )
        {
            if( !_mappings.TryGetValue( sourceInstance, out var dict ) )
            {
                dict = new Dictionary<Type, object>() { { targetType, targetInstance } };
                _mappings.Add( sourceInstance, dict );
            }
            else
            {
#if NET5_0_OR_GREATER
                dict.TryAdd( targetType, targetInstance );
#else
                if( !dict.ContainsKey( targetType ) )
                    dict.Add( targetType, targetInstance );
#endif
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
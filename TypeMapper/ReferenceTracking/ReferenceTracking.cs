using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    /// <summary>
    /// This class helps tracking and retrieving each reference to the mapped type instance.
    /// A reference value type can be mapped to many different types (one instance for each target type).
    /// </summary>
    //public class ReferenceTracking //: IReferenceTracking
    //{
    //    private Dictionary<object, Dictionary<Type, object>> _mappings
    //        = new Dictionary<object, Dictionary<Type, object>>( 8 );

    //    public void Add( object sourceInstance, Type targetType, object targetInstance )
    //    {
    //        Dictionary<Type, object> targetInstances;
    //        if( !_mappings.TryGetValue( sourceInstance, out targetInstances ) )
    //        {
    //            targetInstances = new Dictionary<Type, object>( 2 );
    //            _mappings.Add( sourceInstance, targetInstances );
    //        }

    //        targetInstances.Add( targetType, targetInstance );
    //    }

    //    public bool Contains( object sourceInstance, Type targetType )
    //    {
    //        object targetInstance;
    //        return this.TryGetValue( sourceInstance, targetType, out targetInstance );
    //    }

    //    public bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance )
    //    {
    //        Dictionary<Type, object> targetInstances;
    //        if( !_mappings.TryGetValue( sourceInstance, out targetInstances ) )
    //        {
    //            targetInstance = null;
    //            return false;
    //        }

    //        return targetInstances.TryGetValue( targetType, out targetInstance );
    //    }

    //    public object this[ object sourceInstance, Type targetType ]
    //    {
    //        get { return _mappings[ sourceInstance ][ targetType ]; }
    //    }
    //}

    /// <summary>
    /// This class helps tracking and retrieving each reference to the mapped type instance.
    /// A reference value type can be mapped to many different types (one instance for each target type).
    /// </summary>
    public class ReferenceTracking //: IReferenceTracking
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
            return this.Contains( sourceInstance, targetType );
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

    /// <summary>
    /// This class helps tracking and retrieving each reference to the mapped type instance.
    /// A reference value type can be mapped to many different types (one instance for each target type).
    /// </summary>
    //public class ObjectHashcodeCacheReferenceTracking //: IReferenceTracking
    //{
    //    private Dictionary<int, object> _mappings
    //        = new Dictionary<int, object>( 8 );

    //    private Dictionary<object, int> _objectHashes = new Dictionary<object, int>();

    //    public void Add( object sourceInstance, Type targetType, object targetInstance )
    //    {
    //        int sourceObjectHash;
    //        if( !_objectHashes.TryGetValue( sourceInstance, out sourceObjectHash ) )
    //            _objectHashes.Add( sourceInstance, sourceObjectHash = sourceInstance.GetHashCode() );

    //        var key = sourceObjectHash ^ targetType.GetHashCode();

    //        if( !_mappings.ContainsKey( key ) )
    //            _mappings.Add( key, targetInstance );
    //    }

    //    public bool Contains( object sourceInstance, Type targetType )
    //    {
    //        return this.Contains( sourceInstance, targetType );
    //    }

    //    public bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance )
    //    {
    //        int sourceObjectHash;
    //        if( !_objectHashes.TryGetValue( sourceInstance, out sourceObjectHash ) )
    //            _objectHashes.Add( sourceInstance, sourceObjectHash = sourceInstance.GetHashCode() );

    //        var key = sourceObjectHash ^ targetType.GetHashCode();
    //        return _mappings.TryGetValue( key, out targetInstance );
    //    }

    //    public object this[ object sourceInstance, Type targetType ]
    //    {
    //        get
    //        {
    //            int sourceObjectHash;
    //            if( !_objectHashes.TryGetValue( sourceInstance, out sourceObjectHash ) )
    //                _objectHashes.Add( sourceInstance, sourceObjectHash = sourceInstance.GetHashCode() );

    //            var key = sourceObjectHash ^ targetType.GetHashCode();
    //            return _mappings[ key ];
    //        }
    //    }
    //}
}

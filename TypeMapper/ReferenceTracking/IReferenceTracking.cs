using System;

namespace TypeMapper
{
    public interface IReferenceTracking
    {
        void Add( object sourceInstance, Type targetType, object targetInstance );
        bool Contains( object sourceInstance, Type targetType );
        bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance );
        object this[ object sourceInstance, Type targetType ] { get; }
    }
}
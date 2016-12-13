using System;

namespace TypeMapper
{
    /// <summary>
    /// This interface is unused until a real need shows up
    /// because of virtual calls performance 
    /// </summary>
    public interface IReferenceTracking
    {
        void Add( object sourceInstance, Type targetType, object targetInstance );
        bool Contains( object sourceInstance, Type targetType );
        bool TryGetValue( object sourceInstance, Type targetType, out object targetInstance );
        object this[ object sourceInstance, Type targetType ] { get; }
    }
}
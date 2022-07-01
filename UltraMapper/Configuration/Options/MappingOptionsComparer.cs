using System.Collections.Generic;

namespace UltraMapper.Internals
{
    internal class MappingOptionsComparer : IEqualityComparer<IMappingOptions>
    {
        public bool Equals( IMappingOptions x, IMappingOptions y )
        {
            if( x == null && y == null ) return true;
            if( x == null || y == null ) return false;

            return x.CollectionBehavior == y.CollectionBehavior &&
                x.ReferenceBehavior == y.ReferenceBehavior &&
                x.IsReferenceTrackingEnabled == y.IsReferenceTrackingEnabled;
        }

        public int GetHashCode( IMappingOptions obj )
        {
            return (obj?.CollectionBehavior.GetHashCode() ?? 0) ^
                (obj?.ReferenceBehavior.GetHashCode() ?? 0) ^
                (obj?.IsReferenceTrackingEnabled.GetHashCode() ?? 0);
        }
    }
}

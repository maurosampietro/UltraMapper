using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public class PropertyInfoPair
    {
        public readonly PropertyInfo SourceProperty;
        public readonly PropertyInfo DestinationProperty;
        private readonly int _hashcode;

        public PropertyInfoPair( PropertyInfo sourceType, PropertyInfo destinatinationType )
        {
            this.SourceProperty = sourceType;
            this.DestinationProperty = destinatinationType;

            _hashcode = unchecked(SourceProperty.GetHashCode() * 31)
                ^ DestinationProperty.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as PropertyInfoPair;
            if( typePair == null ) return false;

            return this.SourceProperty.Equals( typePair.SourceProperty ) &&
                this.DestinationProperty.Equals( typePair.DestinationProperty );
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }
    }
}

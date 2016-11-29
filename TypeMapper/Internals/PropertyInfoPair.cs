using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    internal class PropertyInfoPair
    {
        public readonly PropertyInfo SourceProperty;
        public readonly PropertyInfo TargetProperty;

        private readonly int _hashcode;

        public PropertyInfoPair( PropertyInfo sourceType, PropertyInfo destinatinationType )
        {
            this.SourceProperty = sourceType;
            this.TargetProperty = destinatinationType;

            _hashcode = unchecked(this.SourceProperty.GetHashCode() * 31)
                ^ this.TargetProperty.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as PropertyInfoPair;
            if( typePair == null ) return false;

            return this.SourceProperty.Equals( typePair.SourceProperty ) &&
                this.TargetProperty.Equals( typePair.TargetProperty );
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public class TypePair
    {
        public readonly Type SourceType;
        public readonly Type DestinationType;
        private readonly int _hashcode;

        public TypePair( Type sourceType, Type destinatinationType )
        {
            this.SourceType = sourceType;
            this.DestinationType = destinatinationType;

            _hashcode = unchecked(SourceType.GetHashCode() * 31)
                ^ DestinationType.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as TypePair;
            if( typePair == null ) return false;

            return this.SourceType.Equals( typePair.SourceType ) &&
                this.DestinationType.Equals( typePair.DestinationType );
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }
    }
}

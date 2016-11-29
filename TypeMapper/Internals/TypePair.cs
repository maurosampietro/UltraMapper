using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    internal class TypePair
    {
        public readonly Type SourceType;
        public readonly Type TargetType;

        private readonly int _hashcode;

        public TypePair( Type sourceType, Type destinatinationType )
        {
            this.SourceType = sourceType;
            this.TargetType = destinatinationType;

            _hashcode = unchecked(this.SourceType.GetHashCode() * 31)
                ^ this.TargetType.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as TypePair;
            if( typePair == null ) return false;

            return this.SourceType.Equals( typePair.SourceType ) &&
                this.TargetType.Equals( typePair.TargetType );
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }
    }
}

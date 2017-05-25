using System;
using UltraMapper.Internals;

namespace UltraMapper.Internals
{
    public struct TypePair
    {
        public readonly Type SourceType;
        public readonly Type TargetType;

        private readonly Lazy<string> _toString;
        private int? _hashcode;

        public TypePair( Type source, Type target )
        {
            this.SourceType = source;
            this.TargetType = target;

            _toString = new Lazy<string>( () =>
            {
                string sourceTypeName = source.GetPrettifiedName();
                string targetTypeName = target.GetPrettifiedName();

                return $"[{sourceTypeName}, {targetTypeName}]";
            } );

            _hashcode = null;
        }

        public override bool Equals( object obj )
        {
            var typePair = (TypePair)obj;

            return this.SourceType.Equals( typePair.SourceType ) &&
                this.TargetType.Equals( typePair.TargetType );
        }

        public override int GetHashCode()
        {
            if( _hashcode == null )
            {
                _hashcode = unchecked(this.SourceType.GetHashCode() * 31)
                    ^ this.TargetType.GetHashCode();
            }

            return _hashcode.Value;
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

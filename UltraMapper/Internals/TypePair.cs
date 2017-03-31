using System;

namespace UltraMapper.Internals
{
    public struct TypePair
    {
        public readonly Type SourceType;
        public readonly Type TargetType;

        private readonly Lazy<string> _toString;
        private int _hashcode;

        public TypePair( Type sourceType, Type targetType )
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;

            _toString = new Lazy<string>( () =>
            {
                string sourceTypeName = sourceType.GetPrettifiedName();
                string targetTypeName = targetType.GetPrettifiedName();

                return $"[{sourceTypeName}, {targetTypeName}]";
            } );

            _hashcode = 0;
        }

        public override bool Equals( object obj )
        {
            var typePair = (TypePair)obj;

            return this.SourceType.Equals( typePair.SourceType ) &&
                this.TargetType.Equals( typePair.TargetType );
        }

        public override int GetHashCode()
        {
            if( _hashcode == 0 )
            {
                _hashcode = unchecked(this.SourceType.GetHashCode() * 31)
                    ^ this.TargetType.GetHashCode();
            }

            return _hashcode;
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

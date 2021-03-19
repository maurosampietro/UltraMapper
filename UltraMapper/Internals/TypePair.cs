using System;
using System.Text;

namespace UltraMapper.Internals
{
    public struct TypePair
    {
        public readonly Type SourceType;
        public readonly Type TargetType;

        private string _toString;
        private int? _hashcode;

        public TypePair( Type source, Type target )
        {
            this.SourceType = source;
            this.TargetType = target;

            _toString = null;
            _hashcode = null;
        }

        public override bool Equals( object obj )
        {
            if( obj is TypePair typePair )
            {
                return this.SourceType.Equals( typePair.SourceType ) &&
                    this.TargetType.Equals( typePair.TargetType );
            }

            return false;
        }

        public override int GetHashCode()
        {
            if( _hashcode == null )
            {
                _hashcode = this.SourceType.GetHashCode()
                    ^ this.TargetType.GetHashCode();
            }

            return _hashcode.Value;
        }

        public static bool operator !=( TypePair obj1, TypePair obj2 )
        {
            return !(obj1 == obj2);
        }

        public static bool operator ==( TypePair obj1, TypePair obj2 )
        {
            return obj1.Equals( obj2 );
        }

        public override string ToString()
        {
            if( _toString == null )
            {
                string sourceTypeName = this.SourceType.GetPrettifiedName();
                string targetTypeName = this.TargetType.GetPrettifiedName();

                _toString = $"[{sourceTypeName}, {targetTypeName}]";
            }

            return _toString;
        }
    }
}

using System;

namespace TypeMapper.Internals
{
    public class TypePair
    {
        public readonly Type SourceType;
        public readonly Type TargetType;

        private readonly int _hashcode;
        private readonly Lazy<string> _toString;

        public TypePair( Type sourceType, Type targetType )
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;

            _hashcode = unchecked(this.SourceType.GetHashCode() * 31)
                ^ this.TargetType.GetHashCode();

            _toString = new Lazy<string>( () =>
            {
                string sourceTypeName = this.SourceType.GetPrettifiedName();
                string targetTypeName = this.TargetType.GetPrettifiedName();

                return $"[{sourceTypeName}, {targetTypeName}]";
            } );
        }

        public override bool Equals( object obj )
        {
            var typePair = (TypePair)obj;

            return this.SourceType.Equals( typePair.SourceType ) &&
                this.TargetType.Equals( typePair.TargetType );
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

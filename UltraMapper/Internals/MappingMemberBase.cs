using System;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Internals
{
    public abstract class MappingMemberBase
    {
        public readonly MemberInfo MemberInfo;
        public readonly Type MemberType;

        private readonly Lazy<string> _toString;

        public bool Ignore { get; set; }

        internal MappingMemberBase( MemberInfo memberInfo )
        {
            this.MemberInfo = memberInfo;
            this.MemberType = this.MemberInfo.GetMemberType();

            _toString = new Lazy<string>( () =>
            {
                string typeName = this.MemberType.GetPrettifiedName();
                return $"{typeName} {this.MemberInfo.Name}";
            } );
        }

        public override bool Equals( object obj )
        {
            var propertyBase = obj as MappingMemberBase;
            if( propertyBase == null ) return false;

            return this.MemberInfo.Equals( propertyBase.MemberInfo );
        }

        public override int GetHashCode()
        {
            return this.MemberInfo.GetHashCode();
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

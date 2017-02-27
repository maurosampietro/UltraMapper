using System;
using System.Reflection;

namespace TypeMapper.Internals
{
    public abstract class MappingMemberBase
    {
        public readonly MemberInfo MemberInfo;
        public readonly Type MemberType;

        private readonly Lazy<string> _toString;

        //These info are evaluated at configuration level only once for performance reasons
        public bool IsEnumerable { get; set; }
        public bool IsBuiltInType { get; set; }
        public bool IsNullable { get; set; }
        public Type NullableUnderlyingType { get; set; }

        public bool Ignore { get; set; }

        internal MappingMemberBase( MemberInfo memberInfo )
        {
            this.MemberInfo = memberInfo;
            this.MemberType = this.MemberInfo.GetMemberType();

            this.NullableUnderlyingType = Nullable.GetUnderlyingType( this.MemberType );
            this.IsNullable = this.NullableUnderlyingType != null;
            this.IsBuiltInType = this.MemberType.IsBuiltInType( false );
            this.IsEnumerable = this.MemberType.IsEnumerable();

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

using System;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Internals
{
    public abstract class MappingMemberBase
    {
        public readonly MemberAccessPath MemberAccessPath;
        public readonly MemberInfo MemberInfo;
        public readonly Type MemberType;
        private string _toString;

        public bool Ignore { get; set; }

        internal MappingMemberBase( MemberAccessPath memberAccessPath )
        {
            this.MemberAccessPath = memberAccessPath;
            this.MemberInfo = memberAccessPath.Last();
            this.MemberType = this.MemberInfo.GetMemberType();
        }

        public override bool Equals( object obj )
        {
            if( obj is MappingMemberBase propertyBase )
                return this.MemberInfo.Equals( propertyBase.MemberInfo );

            return false;
        }

        public override int GetHashCode()
        {
            return this.MemberInfo.GetHashCode();
        }

        public override string ToString()
        {
            if( _toString == null )
            {
                string typeName = this.MemberType.GetPrettifiedName();
                _toString = $"{typeName} {this.MemberInfo.Name}";
            }

            return _toString;
        }
    }
}

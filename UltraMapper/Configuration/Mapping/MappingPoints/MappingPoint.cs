using System;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Internals
{
    public abstract class MappingPoint : IMappingPoint
    {
        private string _toString;

        public bool Ignore { get; set; }
        public MemberAccessPath MemberAccessPath { get; }
        public readonly MemberInfo MemberInfo;
        public Type MemberType { get; }

        public Type EntryType => this.MemberAccessPath.EntryInstance;
        public Type ReturnType => this.MemberAccessPath.ReturnType;

        public MappingPoint( MemberAccessPath memberAccessPath )
        {
            this.MemberAccessPath = memberAccessPath;
            this.MemberInfo = memberAccessPath.LastOrDefault() ?? memberAccessPath.EntryInstance;
            this.MemberType = this.MemberInfo.GetMemberType();
        }

        public override bool Equals( object obj )
        {
            if( obj is MappingPoint mp )
            {
                return this.MemberInfo.Equals( mp.MemberInfo ) &&
                    this.MemberAccessPath.EntryInstance.Equals( mp.MemberAccessPath.EntryInstance ) &&
                    this.MemberAccessPath.ReturnType.Equals( mp.MemberAccessPath.ReturnType );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.MemberInfo.GetHashCode() ^
                this.MemberAccessPath.EntryInstance?.GetHashCode() ?? 0 ^
                this.MemberAccessPath.ReturnType?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            if( _toString == null )
            {
                string typeName = this.MemberType.GetPrettifiedName();
                _toString = this.MemberInfo == this.MemberType ? typeName
                    : $"{typeName} {this.MemberInfo.Name}";
            }

            return _toString;
        }
    }
}

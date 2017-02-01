using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class MemberInfoPair
    {
        public readonly MemberInfo SourceMember;
        public readonly MemberInfo TargetMember;

        private readonly int _hashcode;
        private readonly Lazy<string> _toString;

        public MemberInfoPair( MemberInfo sourceType, MemberInfo targetType )
        {
            this.SourceMember = sourceType;
            this.TargetMember = targetType;

            _hashcode = unchecked(this.SourceMember.GetHashCode() * 31)
                ^ this.TargetMember.GetHashCode();

            _toString = new Lazy<string>( () =>
            {
                string sourceName = this.SourceMember.Name;
                string sourceTypeName = this.SourceMember.GetMemberType().GetPrettifiedName();

                string targetName = this.TargetMember.Name;
                string targetTypeName = this.TargetMember.GetMemberType().GetPrettifiedName();

                return $"[{sourceTypeName} {sourceName}, {targetTypeName} {targetName}]";
            } );
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as MemberInfoPair;
            if( typePair == null ) return false;

            return this.SourceMember.Equals( typePair.SourceMember ) &&
                this.TargetMember.Equals( typePair.TargetMember );
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

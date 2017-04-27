using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals
{
    public struct MemberPair
    {
        public readonly MemberAccessPath SourceMemberAccess;
        public readonly MemberAccessPath TargetMemberAccess;

        public MemberPair( MemberAccessPath source, MemberAccessPath target )
        {
            this.SourceMemberAccess = source;
            this.TargetMemberAccess = target;
        }

        public MemberPair( MemberAccessPath source, MemberInfo target )
        {
            this.SourceMemberAccess = source;

            this.TargetMemberAccess = new MemberAccessPath();
            this.TargetMemberAccess.Add( target );
        }

        public MemberPair( MemberInfo source, MemberAccessPath target )
        {
            this.SourceMemberAccess = new MemberAccessPath();
            this.SourceMemberAccess.Add( source );

            this.TargetMemberAccess = target;
        }

        public MemberPair( MemberInfo source, MemberInfo target )
        {
            this.SourceMemberAccess = new MemberAccessPath();
            this.SourceMemberAccess.Add( source );

            this.TargetMemberAccess = new MemberAccessPath();
            this.TargetMemberAccess.Add( target );
        }
    }
}

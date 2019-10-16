using System.Reflection;

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
            this.TargetMemberAccess = new MemberAccessPath { target };
        }

        public MemberPair( MemberInfo source, MemberAccessPath target )
        {
            this.SourceMemberAccess = new MemberAccessPath { source };
            this.TargetMemberAccess = target;
        }

        public MemberPair( MemberInfo source, MemberInfo target )
        {
            this.SourceMemberAccess = new MemberAccessPath { source };
            this.TargetMemberAccess = new MemberAccessPath { target };
        }
    }
}

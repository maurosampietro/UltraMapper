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

        public override bool Equals( object obj )
        {
            if( obj is MemberPair memberPair )
            {
                return this.SourceMemberAccess.Equals( memberPair.SourceMemberAccess ) &&
                    this.TargetMemberAccess.Equals( memberPair.TargetMemberAccess );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.SourceMemberAccess.GetHashCode() ^
                 this.TargetMemberAccess.GetHashCode();
        }

        public static bool operator ==( MemberPair left, MemberPair right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( MemberPair left, MemberPair right )
        {
            return !(left == right);
        }
    }
}

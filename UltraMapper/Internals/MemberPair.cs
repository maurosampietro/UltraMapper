using System.Reflection;

namespace UltraMapper.Internals
{
    public struct MemberPair
    {
        public readonly MemberAccessPath SourceMemberPath;
        public readonly MemberAccessPath TargetMemberPath;

        public MemberPair( MemberAccessPath source, MemberAccessPath target )
        {
            this.SourceMemberPath = source;
            this.TargetMemberPath = target;
        }

        public MemberPair( MemberInfo source, MemberInfo target )
        {
            this.SourceMemberPath = new MemberAccessPath { source };
            this.TargetMemberPath = new MemberAccessPath { target };
        }

        public MemberPair( MemberAccessPath source, MemberInfo target )
        {
            this.SourceMemberPath = source;
            this.TargetMemberPath = new MemberAccessPath { target };
        }

        public MemberPair( MemberInfo source, MemberAccessPath target )
        {
            this.SourceMemberPath = new MemberAccessPath { source };
            this.TargetMemberPath = target;
        }

        public override bool Equals( object obj )
        {
            if( obj is MemberPair memberPair )
            {
                return this.SourceMemberPath.Equals( memberPair.SourceMemberPath ) &&
                    this.TargetMemberPath.Equals( memberPair.TargetMemberPath );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.SourceMemberPath.GetHashCode() ^
                 this.TargetMemberPath.GetHashCode();
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

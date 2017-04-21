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
        public readonly MemberInfo SourceMember;
        public readonly MemberInfo TargetMember;

        public MemberPair( MemberInfo source, MemberInfo target )
        {
            this.SourceMember = source;
            this.TargetMember = target;
        }
    }
}

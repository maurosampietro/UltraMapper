using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public interface IConventionResolver
    {
        IMemberProvider SourceMemberProvider { get; }
        IMemberProvider TargetMemberProvider { get; }

        IEnumerable<MemberPair> Resolve( Type source, Type target );
    }
}

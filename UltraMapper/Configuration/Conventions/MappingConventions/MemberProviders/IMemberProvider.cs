using System;
using System.Collections.Generic;
using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMemberProvider
    {
        bool IgnoreFields { get; set; }
        bool IgnoreProperties { get; set; }
        bool IgnoreMethods { get; set; }
        bool IgnoreNonPublicMembers { get; set; }

        IEnumerable<MemberInfo> GetMembers( Type type );
    }
}

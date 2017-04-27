using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

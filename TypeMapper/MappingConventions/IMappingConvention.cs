using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.MappingConventions;

namespace TypeMapper.MappingConventions
{
    public interface IMappingConvention
    {
        PropertyMatchingConfiguration PropertyMatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

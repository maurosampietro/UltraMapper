using System.Reflection;

namespace TypeMapper.MappingConventions
{
    public interface IMatchingRule
    {
        bool IsCompliant( MemberInfo source, MemberInfo target );
    }
}

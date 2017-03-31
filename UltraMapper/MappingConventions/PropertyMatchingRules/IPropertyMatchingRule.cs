using System.Reflection;

namespace UltraMapper.MappingConventions
{
    public interface IMatchingRule
    {
        bool IsCompliant( MemberInfo source, MemberInfo target );
    }
}

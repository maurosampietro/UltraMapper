using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMatchingRule
    {
        bool IsCompliant( MemberInfo source, MemberInfo target );
    }
}

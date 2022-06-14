using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMatchingRule
    {
        bool CanHandle( MemberInfo source, MemberInfo target );
        bool IsCompliant( MemberInfo source, MemberInfo target );
    }
}

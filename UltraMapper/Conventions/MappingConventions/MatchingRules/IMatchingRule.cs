using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMatchingRule
    {
        bool IsCompliant( MemberInfo source, MemberInfo target );
    }

    public interface ITypeMatchingRule : IMatchingRule
    {

    }

    public interface INameMatchingRule : IMatchingRule
    {
        bool IgnoreCase { get; set; }
    }
}

using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMappingConvention
    {
        MatchingRuleConfiguration MatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

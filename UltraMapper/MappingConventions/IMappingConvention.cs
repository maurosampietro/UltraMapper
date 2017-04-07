using System.Reflection;

namespace UltraMapper.MappingConventions
{
    public interface IMappingConvention
    {
        MatchingRuleConfiguration MatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

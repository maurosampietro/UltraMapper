using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMappingConvention
    {
        MatchingRules MatchingRules { get; set; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

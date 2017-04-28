using System.Collections.Generic;
using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMatchingRuleEvaluator
    {
        IEnumerable<IMatchingRule> MatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

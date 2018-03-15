using System.Collections.Generic;
using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMatchingRulesEvaluator
    {
        IEnumerable<IMatchingRule> MatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

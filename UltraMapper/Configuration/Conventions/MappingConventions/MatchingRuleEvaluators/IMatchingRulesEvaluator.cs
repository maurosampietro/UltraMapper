using System.Collections.Generic;
using System.Reflection;

namespace UltraMapper.Conventions
{
    public interface IMatchingRulesEvaluator
    {
        bool IsMatch( MemberInfo source, MemberInfo target, IEnumerable<IMatchingRule> matchingRules );
    }
}

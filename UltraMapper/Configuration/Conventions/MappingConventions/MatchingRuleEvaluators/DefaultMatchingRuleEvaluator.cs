using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Each rule implements <see cref="IMatchingRule"/> or a derived interface.
    /// Rules are grouped by interface type and evaluated.
    /// Each group must have at least one compliant rule to validate a mapping.
    /// </summary>
    public class DefaultMatchingRuleEvaluator : IMatchingRulesEvaluator
    {
        private IEnumerable<IMatchingRule> _lastMatchingRuleSet;
        private List<IGrouping<Type, IMatchingRule>> _ruleGroups;

        public bool IsMatch( MemberInfo source, MemberInfo target, IEnumerable<IMatchingRule> matchingRules )
        {
            //try not to degrade performance too much by caching the last grouped set of rules
            if( _lastMatchingRuleSet != matchingRules )
            {
                Type ruleType( IMatchingRule rule ) => rule.GetType().GetInterfaces()
                    .First( @interface => typeof( IMatchingRule ).IsAssignableFrom( @interface ) );

                _ruleGroups = matchingRules.GroupBy( ruleType ).ToList();
                _lastMatchingRuleSet = matchingRules;
            }

            return _ruleGroups.All( ruleGroup =>
                ruleGroup.Any( rule => rule.IsCompliant( source, target ) ) );
        }
    }
}

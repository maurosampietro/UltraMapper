using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Rules are grouped by interface type.
    /// Each rule implements <see cref="IMatchingRule"/> or a derived interface.
    /// Each group must have at least one compliant rule to validate a mapping.
    /// </summary>
    public class DefaultMatchingRuleEvaluator : IMatchingRuleEvaluator
    {
        private IEnumerable<IGrouping<Type, IMatchingRule>> _ruleGroups;

        public IEnumerable<IMatchingRule> MatchingRules { get; private set; }

        public DefaultMatchingRuleEvaluator( MatchingRules matchingRules )
        {
            if( matchingRules != null && matchingRules.Any() )
            {
                this.MatchingRules = new ReadOnlyCollection<IMatchingRule>( matchingRules.ToList() );

                Func<IMatchingRule, Type> ruleType = rule => rule.GetType().GetInterfaces()
                     .First( @interface => typeof( IMatchingRule ).IsAssignableFrom( @interface ) );

                _ruleGroups = this.MatchingRules.GroupBy( ruleType ).ToList();
            }
        }

        public bool IsMatch( MemberInfo source, MemberInfo target )
        {
            if( this.MatchingRules == null || !this.MatchingRules.Any() ) return true;

            return _ruleGroups.All( ruleGroup =>
                ruleGroup.Any( rule => rule.IsCompliant( source, target ) ) );
        }
    }
}

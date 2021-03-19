using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private IEnumerable<IGrouping<Type, IMatchingRule>> _ruleGroups;

        public IEnumerable<IMatchingRule> MatchingRules { get; private set; }

        public DefaultMatchingRuleEvaluator( MatchingRules matchingRules )
        {
            if( matchingRules == null )
                throw new ArgumentNullException( nameof( matchingRules ) );

            this.MatchingRules = new ReadOnlyCollection<IMatchingRule>( matchingRules.ToList() );

            Type ruleType( IMatchingRule rule ) => rule.GetType().GetInterfaces()
                 .First( @interface => typeof( IMatchingRule ).IsAssignableFrom( @interface ) );

            _ruleGroups = this.MatchingRules.GroupBy( ruleType ).ToList();
        }

        public bool IsMatch( MemberInfo source, MemberInfo target )
        {
            return _ruleGroups.All( ruleGroup =>
                ruleGroup.Any( rule => rule.IsCompliant( source, target ) ) );
        }
    }
}

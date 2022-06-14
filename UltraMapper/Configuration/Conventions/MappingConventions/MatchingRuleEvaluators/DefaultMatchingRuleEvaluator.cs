using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Each rule implements one or more rules of type <see cref="IMatchingRule"/> (or a derived interface).
    /// Rules are grouped by interface type and evaluated.
    /// Each group must have at least one compliant rule to validate a mapping.
    /// </summary>
    public class DefaultMatchingRuleEvaluator : IMatchingRulesEvaluator
    {
        private IEnumerable<IMatchingRule> _lastMatchingRuleSet;
        private Dictionary<Type, List<IMatchingRule>> _ruleGroups
            = new Dictionary<Type, List<IMatchingRule>>();

        public bool IsMatch( MemberInfo source, MemberInfo target, IEnumerable<IMatchingRule> matchingRules )
        {
            //try not to degrade performance too much by caching the last grouped set of rules
            if( _lastMatchingRuleSet != matchingRules )
            {
                foreach( var rule in matchingRules )
                {
                    var ruleTypes = rule.GetType().GetInterfaces()
                        .Where( @interface => typeof( IMatchingRule ).IsAssignableFrom( @interface ) );

                    foreach( var ruletype in ruleTypes )
                    {
                        if( !_ruleGroups.TryGetValue( ruletype, out var rules ) )
                            _ruleGroups.Add( ruletype, rules = new List<IMatchingRule>() );

                        rules.Add( rule );
                    }
                }

                //To avoid evaluating the same rules twice as an IMatchingRule (the base matching rule interface),
                //we remove from the IMatchingRule group the rules already being part of other groups as derived types.
                if( _ruleGroups.TryGetValue( typeof( IMatchingRule ), out var baseInterfaceRules ) )
                {
                    foreach( var group in _ruleGroups )
                    {
                        if( group.Key != typeof( IMatchingRule ) )
                        {
                            foreach( var rule in group.Value )
                                baseInterfaceRules.Remove( rule );
                        }
                    }

                    if( baseInterfaceRules.Count == 0 )
                        _ruleGroups.Remove( typeof( IMatchingRule ) );
                }

                _lastMatchingRuleSet = matchingRules;
            }

            return _ruleGroups.All( ruleGroup =>
            {
                var relevantRules = ruleGroup.Value.Where( rule => rule.CanHandle( source, target ) );
                return relevantRules.Any( rule => rule.IsCompliant( source, target ) );
            } );
        }
    }
}

using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// By default two members match if they have the same name or 
    /// have the same name plus a prefix (prefix + name) or suffix (name + suffix).
    /// </summary>
    public class DefaultMappingConvention : IMappingConvention
    {
        private IEnumerable<IGrouping<Type, IMatchingRule>> _ruleGroups;

        public MatchingRules MatchingRules { get; set; }

        public DefaultMappingConvention()
        {
            this.MatchingRules = new MatchingRules( cfg =>
            {
                cfg.GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = false )
                   .GetOrAdd<PrefixMatching>( rule => rule.IgnoreCase = false )
                   .GetOrAdd<SuffixMatching>( rule => rule.IgnoreCase = false )
                   .GetOrAdd<MethodMatching>( rule => rule.IgnoreCase = false );
            } );
        }

        public bool IsMatch( MemberInfo source, MemberInfo target )
        {
            //TODO: we evaluate only once: if a rule change its options, options are taken into account
            //but changes to the collection holding the rules are ignored.
            if( _ruleGroups == null )
            {
                _ruleGroups = this.MatchingRules.GroupBy( t => t.GetType().GetInterfaces()
                    .First( type => typeof( IMatchingRule ).IsAssignableFrom( type ) ), value => value );
            }

            return _ruleGroups.All( group => group.Any( e => e.IsCompliant( source, target ) ) );
        }
    }
}

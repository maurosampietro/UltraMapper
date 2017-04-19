using System.Reflection;

namespace UltraMapper.MappingConventions
{
    /// <summary>
    /// Two members match if they have the same name.
    /// </summary>
    public class DefaultMappingConvention : IMappingConvention
    {
        public MatchingRuleConfiguration MatchingRules { get; set; }

        public DefaultMappingConvention()
        {
            this.MatchingRules = new MatchingRuleConfiguration( cfg =>
            {
                cfg.GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = false );
            } );
        }

        public bool IsMatch( MemberInfo source, MemberInfo target )
        {
            return this.MatchingRules.MatchingEvaluator( source, target );
        }
    }
}

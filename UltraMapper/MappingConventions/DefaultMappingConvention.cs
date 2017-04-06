using System.Reflection;

namespace UltraMapper.MappingConventions
{
    /// <summary>
    /// Two members match if they have the same name.
    /// </summary>
    public class DefaultMappingConvention : IMappingConvention
    {
        public PropertyMatchingConfiguration PropertyMatchingRules { get; set; }

        public DefaultMappingConvention()
        {
            this.PropertyMatchingRules = new PropertyMatchingConfiguration( cfg =>
            {
                cfg.GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = false );
            } );
        }

        public bool IsMatch( MemberInfo source, MemberInfo target )
        {
            return this.PropertyMatchingRules
                .MatchingEvaluator( source, target );
        }
    }
}

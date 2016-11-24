using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Inizialize a blank mapping convention.
    /// No property matching rule is applied by default.
    /// </summary>
    public class CustomMappingConvention : IMappingConvention
    {
        public PropertyMatchingConfiguration PropertyMatchingRules { get; set; }

        public CustomMappingConvention()
        {
            this.PropertyMatchingRules = new PropertyMatchingConfiguration();
        }

        public bool IsMatch( PropertyInfo source, PropertyInfo target )
        {
            return this.PropertyMatchingRules.MatchingEvaluator( source, target );
        }
    }
}

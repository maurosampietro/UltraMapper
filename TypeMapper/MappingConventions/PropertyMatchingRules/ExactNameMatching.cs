using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions.PropertyMatchingRules
{
    /// <summary>
    /// Two properties match if they have the same name.
    /// Name case can be optionally ignored.
    /// </summary>
    public class ExactNameMatching : PropertyMatchingRuleBase
    {
        public bool IgnoreCase { get; set; } = false;

        public override bool IsCompliant( PropertyInfo source, PropertyInfo target )
        {
            var comparisonType = this.IgnoreCase ?
              StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return source.Name.Equals( target.Name, comparisonType );
        }
    }
}

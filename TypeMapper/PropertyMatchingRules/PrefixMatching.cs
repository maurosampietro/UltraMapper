using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Two properties match if targetName = suffix + sourceName.
    /// Name case can be optionally ignored.
    /// </summary>
    public class PrefixMatching : PropertyMatchingRuleBase
    {
        public bool IgnoreCase { get; set; }
        public string[] Prefixes { get; set; }

        public PrefixMatching()
            : this( new string[] { "Dto", "DataTransferObject" } ) { }

        public PrefixMatching( params string[] prefixes )
        {
            this.IgnoreCase = false;
            this.Prefixes = prefixes;
        }

        public override bool IsCompliant( PropertyInfo source, PropertyInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this.Prefixes.Any( ( prefix ) =>
                target.Name.Equals( prefix + source.Name, comparisonType ) );
        }
    }
}

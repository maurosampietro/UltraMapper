using System;
using System.Linq;
using System.Reflection;

namespace UltraMapper.MappingConventions
{
    /// <summary>
    /// Two members match if targetName = suffix + sourceName.
    /// </summary>
    public class PrefixMatching : MatchingRuleBase
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

        public override bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this.Prefixes.Any( prefix =>
                target.Name.Equals( prefix + source.Name, comparisonType ) );
        }
    }
}

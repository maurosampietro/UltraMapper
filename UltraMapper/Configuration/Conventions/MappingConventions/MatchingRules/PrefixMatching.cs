using System;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Two members match if (sourceName == prefix + targetName) or (targetName == prefix + sourceName).
    /// </summary>
    public class PrefixMatching : INameMatchingRule
    {
        public bool IgnoreCase { get; set; }
        public string[] Prefixes { get; set; }

        public PrefixMatching()
            : this( new string[] { "Dto", "DataTransferObject" } ) { }

        public PrefixMatching( params string[] prefixes )
        {
            this.IgnoreCase = true;
            this.Prefixes = prefixes;
        }

        public bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this.Prefixes.Any( prefix =>
                source.Name.Equals( prefix + target.Name, comparisonType ) ||
                target.Name.Equals( prefix + source.Name, comparisonType ) );
        }
    }
}

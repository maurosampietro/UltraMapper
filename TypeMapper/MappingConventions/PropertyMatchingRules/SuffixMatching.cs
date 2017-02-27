using System;
using System.Linq;
using System.Reflection;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Two properties match if targetName = sourceName + suffix.
    /// Name case can be optionally ignored.
    /// </summary>
    public class SuffixMatching : PropertyMatchingRuleBase
    {
        public bool IgnoreCase { get; set; }
        public string[] Suffixes { get; set; }

        public SuffixMatching()
            : this( new string[] { "Dto", "DataTransferObject" } ) { }

        public SuffixMatching( params string[] suffixes )
        {
            this.IgnoreCase = false;
            this.Suffixes = suffixes;
        }

        public override bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this.Suffixes.Any( ( suffix ) =>
                target.Name.Equals( source.Name + suffix, comparisonType ) );
        }
    }
}

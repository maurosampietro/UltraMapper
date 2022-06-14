using System;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Two members match if (sourceName == targetName + suffix) or (targetName == sourceName + suffix).
    /// </summary>
    public class SuffixMatching : INameMatchingRule
    {
        public bool IgnoreCase { get; set; }
        public string[] Suffixes { get; set; }

        public SuffixMatching()
            : this( new string[] { "Dto", "DataTransferObject" } ) { }

        public SuffixMatching( params string[] suffixes )
        {
            this.IgnoreCase = true;
            this.Suffixes = suffixes;
        }

        public bool CanHandle( MemberInfo source, MemberInfo target )
        {
            return !(source is MethodInfo) && !(target is MethodInfo);
        }

        public bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return this.Suffixes.Any( suffix =>
                source.Name.Equals( target.Name + suffix, comparisonType ) ||
                target.Name.Equals( source.Name + suffix, comparisonType ) );
        }
    }
}

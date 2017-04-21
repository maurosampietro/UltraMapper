using System;
using System.Reflection;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Two members match if they have the same name.
    /// Name case can be optionally ignored.
    /// </summary>
    public class ExactNameMatching : MatchingRuleBase
    {
        public bool IgnoreCase { get; set; } = false;

        public override bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return source.Name.Equals( target.Name, comparisonType );
        }
    }
}

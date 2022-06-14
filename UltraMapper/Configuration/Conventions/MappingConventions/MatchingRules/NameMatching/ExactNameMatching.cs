using System;
using System.Reflection;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Two members match if they have the same name.
    /// Name case can be optionally ignored.
    /// </summary>
    public class ExactNameMatching : INameMatchingRule
    {
        public bool IgnoreCase { get; set; } = true;

        public bool CanHandle( MemberInfo source, MemberInfo target )
        {
            return !(source is MethodInfo) && !(target is MethodInfo);
        }

        public bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return source.Name.Equals( target.Name, comparisonType );
        }
    }
}

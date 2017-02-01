using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    public abstract class PropertyMatchingRuleBase : IMatchingRule
    {
        public abstract bool IsCompliant( MemberInfo source, MemberInfo target );

        public static RuleChaining operator &( PropertyMatchingRuleBase lhs, PropertyMatchingRuleBase rhs )
        {
            return lhs.And( rhs );
        }

        public static RuleChaining operator &( PropertyMatchingRuleBase lhs, RuleChaining rhs )
        {
            return lhs.And( rhs );
        }

        public static RuleChaining operator &( RuleChaining lhs, PropertyMatchingRuleBase rhs )
        {
            return lhs.And( rhs );
        }

        public static RuleChaining operator |( PropertyMatchingRuleBase lhs, RuleChaining rhs )
        {
            return lhs.Or( rhs );
        }

        public static RuleChaining operator |( RuleChaining lhs, PropertyMatchingRuleBase rhs )
        {
            return lhs.Or( rhs );
        }

        public static RuleChaining operator |( PropertyMatchingRuleBase lhs, PropertyMatchingRuleBase rhs )
        {
            return lhs.Or( rhs );
        }
    }
}

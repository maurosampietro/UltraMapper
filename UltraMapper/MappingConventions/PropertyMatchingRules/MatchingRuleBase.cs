using System.Reflection;

namespace UltraMapper.MappingConventions
{
    public abstract class MatchingRuleBase : IMatchingRule
    {
        public abstract bool IsCompliant( MemberInfo source, MemberInfo target );

        public static RuleChaining operator &( MatchingRuleBase lhs, MatchingRuleBase rhs )
        {
            return lhs.And( rhs );
        }

        public static RuleChaining operator &( MatchingRuleBase lhs, RuleChaining rhs )
        {
            return lhs.And( rhs );
        }

        public static RuleChaining operator &( RuleChaining lhs, MatchingRuleBase rhs )
        {
            return lhs.And( rhs );
        }

        public static RuleChaining operator |( MatchingRuleBase lhs, RuleChaining rhs )
        {
            return lhs.Or( rhs );
        }

        public static RuleChaining operator |( RuleChaining lhs, MatchingRuleBase rhs )
        {
            return lhs.Or( rhs );
        }

        public static RuleChaining operator |( MatchingRuleBase lhs, MatchingRuleBase rhs )
        {
            return lhs.Or( rhs );
        }
    }
}

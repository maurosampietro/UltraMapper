using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class DefaultConvention : IMappingConvention
    {
        public ISourceMemberProvider SourceMemberProvider { get; set; }
        public ITargetMemberProvider TargetMemberProvider { get; set; }

        public IMatchingRulesEvaluator MatchingRulesEvaluator { get; set; }
        public TypeSet<IMatchingRule> MatchingRules { get; set; }

        public DefaultConvention()
        {
            this.SourceMemberProvider = new SourceMemberProvider();
            this.TargetMemberProvider = new TargetMemberProvider();

            this.MatchingRules = new TypeSet<IMatchingRule>( cfg =>
            {
                cfg.GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = true );
            } );
            
            this.MatchingRulesEvaluator = new DefaultMatchingRuleEvaluator();
        }

        public IEnumerable<MemberPair> MapByConvention( Type source, Type target )
        {
            var sourceMembers = this.SourceMemberProvider.GetMembers( source );
            var targetMembers = this.TargetMemberProvider.GetMembers( target ).ToList();

            foreach( var sourceMember in sourceMembers )
            {
                foreach( var targetMember in targetMembers )
                {
                    if( this.MatchingRulesEvaluator.IsMatch( sourceMember, targetMember, this.MatchingRules ) )
                    {
                        yield return new MemberPair( sourceMember, targetMember );
                        break; //sourceMember is now mapped, jump directly to the next sourceMember
                    }
                }
            }
        }
    }
}

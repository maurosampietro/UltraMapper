using System;
using System.Collections.Generic;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public interface IMappingConvention
    {
        TypeSet<IMatchingRule> MatchingRules { get; set; }
        IMatchingRulesEvaluator MatchingRulesEvaluator { get; set; }

        ISourceMemberProvider SourceMemberProvider { get; set; }
        ITargetMemberProvider TargetMemberProvider { get; set; }

        IEnumerable<MemberPair> MapByConvention( Type source, Type target );        
    }
}

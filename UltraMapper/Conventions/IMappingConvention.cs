using System;
using System.Collections.Generic;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public interface IMappingConvention
    {
        MatchingRules MatchingRules { get; set; }
        IMatchingRulesEvaluator MatchingRulesEvaluator { get; set; }

        IMemberProvider SourceMemberProvider { get; set; }
        IMemberProvider TargetMemberProvider { get; set; }

        IEnumerable<MemberPair> MapByConvention( Type source, Type target );        
    }
}

using System;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MatchingRules : SingletonList<IMatchingRule>
    {
        public MatchingRules( Action<SingletonList<IMatchingRule>> config = null )
            : base( config ) { }
    }
}

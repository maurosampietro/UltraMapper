using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MatchingRules : SingletonList<IMatchingRule>
    {
        public MatchingRules( Action<SingletonList<IMatchingRule>> config = null )
            : base( config ) { }
    }
}

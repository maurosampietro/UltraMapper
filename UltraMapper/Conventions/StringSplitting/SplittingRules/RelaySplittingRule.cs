using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Conventions
{
    public class RelaySplittingRule : IStringSplittingRule
    {
        private readonly Func<char, bool> _splittingRule;

        public bool IsSplitChar( char ch ) => _splittingRule( ch );

        public RelaySplittingRule( Func<char, bool> splittingRule )
        {
            _splittingRule = splittingRule;
        }
    }
}

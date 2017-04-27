using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Conventions
{
    public class RelayStringSplittingRule : IStringSplittingRule
    {
        private readonly Func<char, bool> _splittingRule;

        public bool RemoveSplitChar { get; private set; }

        public bool IsSplitChar( char ch ) => _splittingRule( ch );

        public RelayStringSplittingRule( Func<char, bool> splittingRule, bool removeSplitChar )
        {
            this.RemoveSplitChar = removeSplitChar;
            _splittingRule = splittingRule;
        }
    }
}

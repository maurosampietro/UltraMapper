using System;

namespace UltraMapper.Conventions
{
    public class RelayStringSplittingRule : IStringSplittingRule
    {
        private readonly Func<char, bool> _splittingRule;

        public bool RemoveSplitChar { get; private set; }

        public bool IsSplitChar( char ch ) => _splittingRule( ch );

        public RelayStringSplittingRule( Func<char, bool> splittingRule, bool removeSplitChar )
        {
            _splittingRule = splittingRule;
            this.RemoveSplitChar = removeSplitChar;
        }
    }
}

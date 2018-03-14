using System;
using System.Collections.Generic;

namespace UltraMapper.Conventions
{
    public sealed class StringSplitter
    {
        public readonly IStringSplittingRule SplittingRule;

        public StringSplitter( IStringSplittingRule splittingRule )
        {
            this.SplittingRule = splittingRule;
        }

        public IEnumerable<string> Split( string str )
        {
            if( String.IsNullOrEmpty( str ) ) yield break;

            int removeCharOffset = this.SplittingRule.RemoveSplitChar ? 1 : 0;

            int lastSplit = 0;
            for( int i = 1; i < str.Length; i++ )
            {
                if( this.SplittingRule.IsSplitChar( str[ i ] ) )
                {
                    yield return str.Substring( lastSplit, i - lastSplit );
                    lastSplit = i + removeCharOffset;
                }
            }

            if( lastSplit <= str.Length - 1 )
                yield return str.Substring( lastSplit, str.Length - lastSplit );
        }
    }
}

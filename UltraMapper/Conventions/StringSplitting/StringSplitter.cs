using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Conventions
{
    public sealed class StringSplitter
    {
        public IStringSplittingRule SplittingRule { get; set; }

        public StringSplitter( IStringSplittingRule splittingRule )
        {
            this.SplittingRule = splittingRule;
        }

        public IEnumerable<string> Split( string str )
        {
            if( String.IsNullOrEmpty( str ) ) yield break;

            int lastSplit = 0;
            for( int i = 1; i < str.Length; i++ )
            {
                if( this.SplittingRule.IsSplitChar( str[ i ] ) )
                {
                    yield return str.Substring( lastSplit, i - lastSplit );
                    lastSplit = i;
                }
            }

            if( lastSplit <= str.Length - 1 )
                yield return str.Substring( lastSplit, str.Length - lastSplit );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Conventions
{
    /// <summary>
    /// Informs the caller to split if an underscore character is encountered
    /// </summary>
    public class UnderscoreStringSplittingRule : IStringSplittingRule
    {
        public bool IsSplitChar( char c ) => c == '_';
    }
}

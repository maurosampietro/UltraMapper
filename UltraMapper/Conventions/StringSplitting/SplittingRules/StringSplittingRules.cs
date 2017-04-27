using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Conventions;

namespace UltraMapper.Conventions
{
    public static class StringSplittingRules
    {
        /// <summary>
        /// Informs to split if an upper case character is encountered
        /// </summary>
        public static IStringSplittingRule PascalCaseRule =
            new RelayStringSplittingRule( c => Char.IsUpper( c ), removeSplitChar: false );

        /// <summary>
        /// Informs the caller to split if an underscore character is encountered
        /// </summary>
        public static IStringSplittingRule UnderscoreRule =
            new RelayStringSplittingRule( c => c == '_', removeSplitChar: true );
    }
}

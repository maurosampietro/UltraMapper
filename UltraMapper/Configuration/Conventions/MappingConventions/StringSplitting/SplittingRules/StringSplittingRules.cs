using System;

namespace UltraMapper.Conventions
{
    public static class StringSplittingRules
    {
        /// <summary>
        /// Informs to split if an upper case character is encountered
        /// </summary>
        public static IStringSplittingRule PascalCase =
            new RelayStringSplittingRule( c => Char.IsUpper( c ), removeSplitChar: false );

        /// <summary>
        /// Informs the caller to split if an underscore character is encountered.
        /// The underscore itself is not considered part of the split.
        /// </summary>
        public static IStringSplittingRule SnakeCase =
            new RelayStringSplittingRule( c => c == '_', removeSplitChar: true );
    }
}

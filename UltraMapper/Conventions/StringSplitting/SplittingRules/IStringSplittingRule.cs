namespace UltraMapper.Conventions
{
    public interface IStringSplittingRule
    {
        /// <summary>
        /// Set whether remove or keep the splitting char when it is found in the string.
        /// </summary>
        bool RemoveSplitChar { get; }

        /// <summary>
        /// Identifies the splitting char
        /// </summary>
        /// <param name="c">The char to check</param>
        /// <returns>True if the char is a splitting char. False otherwise.</returns>
        bool IsSplitChar( char c );
    }
}

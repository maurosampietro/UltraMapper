namespace UltraMapper.Conventions
{
    public interface IStringSplittingRule
    {
        bool RemoveSplitChar { get; }
        bool IsSplitChar( char c );
    }
}

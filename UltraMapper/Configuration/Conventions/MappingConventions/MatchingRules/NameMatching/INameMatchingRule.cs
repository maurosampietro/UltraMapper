using UltraMapper.Conventions;

namespace UltraMapper.Conventions
{
    public interface INameMatchingRule : IMatchingRule
    {
        bool IgnoreCase { get; set; }
    }
}

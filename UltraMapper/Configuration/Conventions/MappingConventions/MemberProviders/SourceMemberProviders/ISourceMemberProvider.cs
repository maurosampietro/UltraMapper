using UltraMapper.Conventions;

namespace UltraMapper.Conventions
{
    public interface ISourceMemberProvider : IMemberProvider
    {
        bool AllowGetterMethodsOnly { get; }
    }
}

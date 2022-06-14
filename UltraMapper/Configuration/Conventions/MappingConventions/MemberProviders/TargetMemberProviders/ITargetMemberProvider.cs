using UltraMapper.Conventions;

namespace UltraMapper.Conventions
{
    public interface ITargetMemberProvider : IMemberProvider
    {
        bool AllowGetterOrSetterMethodsOnly { get; set; }
    }
}

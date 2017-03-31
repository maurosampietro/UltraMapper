using System.Reflection;

namespace UltraMapper.MappingConventions
{
    public interface IMappingConvention
    {
        PropertyMatchingConfiguration PropertyMatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

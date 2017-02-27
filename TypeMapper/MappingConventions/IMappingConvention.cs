using System.Reflection;

namespace TypeMapper.MappingConventions
{
    public interface IMappingConvention
    {
        PropertyMatchingConfiguration PropertyMatchingRules { get; }
        bool IsMatch( MemberInfo source, MemberInfo target );
    }
}

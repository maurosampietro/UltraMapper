using System.Linq.Expressions;

namespace UltraMapper
{
    public interface IMappingOptions
    {
        CollectionBehaviors CollectionBehavior { get; set; }
        ReferenceBehaviors ReferenceBehavior { get; set; }

        LambdaExpression CollectionItemEqualityComparer { get; set; }
        LambdaExpression CustomTargetConstructor { get; set; }
    }

    public interface ITypeOptions : IMappingOptions
    {
        bool IgnoreMemberMappingResolvedByConvention { get; set; }
    }

    public interface IMemberOptions : IMappingOptions
    {
        bool Ignore { get; set; }
    }
}

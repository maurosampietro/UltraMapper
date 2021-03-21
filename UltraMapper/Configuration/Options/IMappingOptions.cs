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

    public interface ITypeMappingOptions : IMappingOptions
    {
        bool? IgnoreMemberMappingResolvedByConvention { get; set; }
    }

    public interface IMemberMappingOptions : IMappingOptions
    {
        bool Ignore { get; set; }
    }
}

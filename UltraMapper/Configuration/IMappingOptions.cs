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
}

using System;
using System.Linq.Expressions;

namespace UltraMapper
{
    public interface IMappingOptions
    {
        bool IsReferenceTrackingEnabled { get; set; }

        CollectionBehaviors CollectionBehavior { get; set; }
        ReferenceBehaviors ReferenceBehavior { get; set; }

        LambdaExpression CollectionItemEqualityComparer { get; set; }
        LambdaExpression CustomTargetConstructor { get; set; }
        LambdaExpression CustomConverter { get; set; }

        void SetCustomTargetConstructor<T>( Expression<Func<T>> ctor );
        void SetCollectionItemEqualityComparer<TSource, TTarget>( Expression<Func<TSource, TTarget, bool>> converter );
    }
}

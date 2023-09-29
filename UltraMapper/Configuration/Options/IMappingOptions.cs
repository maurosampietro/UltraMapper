using System;
using System.Collections.Generic;
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
        LambdaExpression CustomTargetInsertMethod { get; set; }

        void SetCustomTargetConstructor<T>( Expression<Func<T>> ctor );
        void SetCustomTargetInsertMethod<TTarget, TItem>( Expression<Action<TTarget, TItem>> insert ) where TTarget : IEnumerable<TItem>;
        void SetCollectionItemEqualityComparer<TSource, TTarget>( Expression<Func<TSource, TTarget, bool>> converter );
    }
}

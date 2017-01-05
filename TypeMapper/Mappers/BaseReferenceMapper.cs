using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public abstract class BaseReferenceObjectMapper
    {
        private static Func<ReferenceTracking, object, Type, object> refTrackingLookup =
            ( referenceTracker, sourceInstance, targetType ) =>
        {
            object targetInstance;
            referenceTracker.TryGetValue( sourceInstance, targetType, out targetInstance );

            return targetInstance;
        };

        private static Action<ReferenceTracking, object, Type, object> addToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance, targetType, targetInstance );
        };

        protected static readonly Expression<Func<ReferenceTracking, object, Type, object>> CacheLookupExpression =
            ( rT, sI, tT ) => refTrackingLookup( rT, sI, tT );

        protected static readonly Expression<Action<ReferenceTracking, object, Type, object>> CacheAddExpression =
            ( rT, sI, tT, tI ) => addToTracker( rT, sI, tT, tI );
    }
}

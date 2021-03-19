using System;
using System.Linq.Expressions;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MappingExpressionBuilder
    {
        public static Action<ReferenceTracker, object, object> GetMappingFunc(
            Type source, Type target, LambdaExpression mappingExpression )
        {
            var referenceTrackerParam = Expression.Parameter( typeof( ReferenceTracker ), "referenceTracker" );
            var sourceParam = Expression.Parameter( typeof( object ), "sourceInstance" );
            var targetParam = Expression.Parameter( typeof( object ), "targetInstance" );

            var sourceInstance = Expression.Convert( sourceParam, source );
            var targetInstance = Expression.Convert( targetParam, target );

            if( mappingExpression.Parameters.Count == 2 &&
                mappingExpression.Parameters[ 0 ].Type == typeof( ReferenceTracker ) )
            {
                var bodyExp = Expression.Block
                (
                    Expression.Invoke( mappingExpression, referenceTrackerParam, sourceInstance )
                );

                return Expression.Lambda<Action<ReferenceTracker, object, object>>(
                    bodyExp, referenceTrackerParam, sourceParam, targetParam ).Compile();
            }
            else
            {
                var bodyExp = Expression.Invoke( mappingExpression,
                    referenceTrackerParam, sourceInstance, targetInstance );

                return Expression.Lambda<Action<ReferenceTracker, object, object>>(
                    bodyExp, referenceTrackerParam, sourceParam, targetParam ).Compile();
            }
        }
    }
}

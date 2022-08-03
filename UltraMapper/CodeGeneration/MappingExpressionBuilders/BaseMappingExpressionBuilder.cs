using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class MappingExpressionBuilder
    {
        public static UltraMapperFunc GetMappingEntryPoint(
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

                return Expression.Lambda<UltraMapperFunc>(
                    bodyExp, referenceTrackerParam, sourceParam, targetParam ).Compile();
            }
            else if( mappingExpression.Parameters.Count == 1 )
            {
                var bodyExp = Expression.Convert( Expression.Invoke( mappingExpression, sourceInstance ), typeof( object ) );

                return Expression.Lambda<UltraMapperFunc>(
                    bodyExp, referenceTrackerParam, sourceParam, targetParam ).Compile();
            }
            else
            {
                var bodyExp = Expression.Convert( Expression.Invoke( mappingExpression,
                    referenceTrackerParam, sourceInstance, targetInstance ), typeof( object ) );

                return Expression.Lambda<UltraMapperFunc>(
                    bodyExp, referenceTrackerParam, sourceParam, targetParam ).Compile();
            }
        }
    }
}

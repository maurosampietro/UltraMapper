using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CustomConverterExpressionBuilder
    {
        public static Func<ReferenceTracker, object, Type, object> refTrackingLookup =
            ( referenceTracker, sourceInstance, targetType ) =>
            {
                referenceTracker.TryGetValue( sourceInstance, targetType, out object targetInstance );
                return targetInstance;
            };

        public static Action<ReferenceTracker, object, Type, object> addToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
            {
                referenceTracker.Add( sourceInstance, targetType, targetInstance );
            };

        /// <summary>
        /// Adds reference tracking
        /// </summary>
        /// <param name="converter"></param>
        public static LambdaExpression Encapsule( LambdaExpression customConverter )
        {
            var sourceType = customConverter.Parameters[ 0 ].Type;
            var targetType = customConverter.ReturnType;
            var context = new CustomConverterContext( sourceType, targetType );

            var expression = GetExpression( customConverter, context );

            var delegateType = typeof( Func<,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance );
        }

        private static Expression GetExpression( LambdaExpression customConverter, CustomConverterContext context )
        {
            var sourceType = customConverter.Parameters[ 0 ].Type;
            var targetType = customConverter.ReturnType;

            if( sourceType.IsBuiltIn( true ) || targetType.IsBuiltIn( true ) )
            {
                //If either the source type or the target type is a primitive type then
                //there's nothing to track but the lambda will end up being encapsulated anyway
                return Expression.Invoke( customConverter, context.SourceInstance );
            }

            Expression itemLookupCall = Expression.Call
            (
                Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method,
                context.ReferenceTracker,
                context.SourceInstance,
                Expression.Constant( targetType )
            );

            Expression itemCacheCall = Expression.Call
            (
                Expression.Constant( addToTracker.Target ),
                addToTracker.Method,
                context.ReferenceTracker,
                context.SourceInstance,
                Expression.Constant( targetType ),
                context.TargetInstance
            );

            return Expression.Block
            (
                new[] { context.TrackedReference, context.TargetInstance },

                //object lookup. An intermediate variable (TrackedReference) is needed in order to deal with ReferenceMappingStrategies
                Expression.Assign( context.TrackedReference,
                    Expression.Convert( itemLookupCall, targetType ) ),

                Expression.IfThenElse
                (
                    Expression.Equal( context.TrackedReference, context.TargetNullValue ),

                    Expression.Block
                    (
                        Expression.Assign( context.TargetInstance, customConverter.Body
                            .ReplaceParameter( context.SourceInstance, customConverter.Parameters[ 0 ].Name ) ),

                        //cache reference
                        itemCacheCall
                    ),

                    Expression.Assign( context.TargetInstance, context.TrackedReference )
                ),

                context.TargetInstance
            );
        }
    }
}

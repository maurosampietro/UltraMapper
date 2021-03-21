using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CustomConverterExpressionBuilder
    {
        public static LambdaExpression ReplaceParams( LambdaExpression customConverter )
        {
            if( customConverter.Parameters.Count == 1 ) 
            { 
                //First param: SourceInstance
                var sourceType = customConverter.Parameters[ 0 ].Type;
                var targetType = customConverter.ReturnType;
                var context = new CustomConverterContext( sourceType, targetType );

                var expression = customConverter.Body.ReplaceParameter( context.SourceInstance,
                    customConverter.Parameters[ 0 ].Name );

                var delegateType = typeof( Func<,> ).MakeGenericType(
                    context.SourceInstance.Type, context.TargetInstance.Type );

                return Expression.Lambda( delegateType, expression, context.SourceInstance );
            }
            else if( customConverter.Parameters.Count == 2 )
            {
                //First param: ReferenceTracker
                //Second param: SourceInstance

                var sourceType = customConverter.Parameters[ 1 ].Type;
                var targetType = customConverter.ReturnType;
                var context = new CustomConverterContext( sourceType, targetType );

                var expression = customConverter.Body
                    .ReplaceParameter( context.ReferenceTracker, customConverter.Parameters[ 0 ].Name )
                    .ReplaceParameter( context.SourceInstance, customConverter.Parameters[ 1 ].Name );

                var delegateType = typeof( Func<,,> ).MakeGenericType(
                    context.ReferenceTracker.Type, context.SourceInstance.Type,
                    context.TargetInstance.Type );

                return Expression.Lambda( delegateType, expression,
                    context.ReferenceTracker, context.SourceInstance );
            }

            throw new NotSupportedException( "The type of custom converter you supplied is not supported" );
        }

        ///// <summary>
        ///// Adds reference tracking
        ///// </summary>
        ///// <param name="converter"></param>
        //public static LambdaExpression Encapsule( LambdaExpression customConverter )
        //{
        //    var sourceType = customConverter.Parameters[ 0 ].Type;
        //    var targetType = customConverter.ReturnType;
        //    var context = new CustomConverterContext( sourceType, targetType );

        //    var expression = GetExpression( customConverter, context );

        //    var delegateType = typeof( Func<,,> ).MakeGenericType(
        //        context.ReferenceTracker.Type, context.SourceInstance.Type,
        //        context.TargetInstance.Type );

        //    return Expression.Lambda( delegateType, expression,
        //        context.ReferenceTracker, context.SourceInstance );
        //}

        //private static Expression GetExpression( LambdaExpression customConverter, CustomConverterContext context )
        //{
        //    var sourceType = customConverter.Parameters[ 0 ].Type;
        //    var targetType = customConverter.ReturnType;

        //    if( sourceType.IsBuiltIn( true ) || targetType.IsBuiltIn( true ) )
        //    {
        //        //If either the source type or the target type is a primitive type then
        //        //there's nothing to track but the lambda will end up being encapsulated anyway
        //        return Expression.Invoke( customConverter, context.SourceInstance );
        //    }

        //    var memberAssignment = Expression.Assign( context.TargetInstance, customConverter.Body
        //        .ReplaceParameter( context.SourceInstance, customConverter.Parameters[ 0 ].Name ) );

        //    var lookUpBlock = ReferenceTracking.ReferenceTrackingExpression.GetMappingExpression(
        //        context.ReferenceTracker, context.SourceInstance, context.TargetInstance,
        //        memberAssignment, null, null, null, false );

        //    return Expression.Block
        //    (
        //        new[] { context.TargetInstance },
        //        lookUpBlock,
        //        context.TargetInstance
        //    );
        //}
    }
}

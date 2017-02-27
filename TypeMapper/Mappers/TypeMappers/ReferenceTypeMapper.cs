using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.TypeMappers
{
    public class ReferenceMapperTypeMapping
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

        public virtual bool CanHandle( TypeMapping mapping )
        {
            return !mapping.TypePair.SourceType.IsValueType &&
                !mapping.TypePair.TargetType.IsValueType &&
                !mapping.TypePair.SourceType.IsBuiltInType( false ) &&
                !mapping.TypePair.TargetType.IsBuiltInType( false ) &&
                !mapping.TypePair.SourceType.IsEnumerable();
        }

        public LambdaExpression GetMappingExpression( TypeMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, ObjectPair>

            var context = this.GetMapperContext( mapping ) as ReferenceMapperContextTypeMapping;
            var expressionBody = this.GetExpressionBody( context );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceType, context.TargetType, context.ReturnType );

            return Expression.Lambda( delegateType, expressionBody,
                context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }

        protected virtual object GetMapperContext( TypeMapping mapping )
        {
            return new ReferenceMapperContextTypeMapping( mapping );
        }

        protected virtual Expression ReturnTypeInitialization( object contextObj )
        {
            var context = contextObj as ReferenceMapperContextTypeMapping;
            return Expression.Assign( context.ReturnObjectVar, Expression.Constant( null, context.ReturnType ) );
        }

        protected Expression GetExpressionBody( ReferenceMapperContextTypeMapping context )
        {
            /* SOURCE (NULL) -> TARGET = NULL
            * 
            * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
            * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
            * 
            * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NULL) = ASSIGN NEW OBJECT 
            * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET(NOT NULL) = KEEP USING INSTANCE OR CREATE NEW OBJECT
            */

            var body = (Expression)Expression.Block
            (
                new ParameterExpression[] { context.SourcePropertyVar, context.TargetPropertyVar, context.ReturnObjectVar },

                ReturnTypeInitialization( context ),

                //read source value
                Expression.Assign( context.SourcePropertyVar, context.SourceInstance ),

                Expression.IfThenElse
                (
                     Expression.Equal( context.SourcePropertyVar, context.SourceNullValue ),

                     Expression.Assign( context.TargetPropertyVar, context.TargetNullValue ),

                     Expression.Block
                     (
                        //object lookup
                        //Expression.Assign( context.TargetPropertyVar, Expression.Convert(
                        //    Expression.Invoke( CacheLookupExpression, context.ReferenceTrack, context.SourcePropertyVar,
                        //    Expression.Constant( context.TargetPropertyType ) ), context.TargetPropertyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetPropertyVar, context.TargetNullValue ),
                            Expression.Block
                            (
                                this.GetInnerBody( context )

                            //cache reference
                            //Expression.Invoke( CacheAddExpression, context.ReferenceTrack, context.SourcePropertyVar,
                            //    Expression.Constant( context.TargetPropertyType ), context.TargetPropertyVar )
                            )
                        )
                    )
                ),

                //context.Mapping.TargetProperty.ValueSetter.Body
                //    .ReplaceParameter( context.TargetInstance, "target" )
                //    .ReplaceParameter( context.TargetPropertyVar, "value" ),

                context.ReturnObjectVar
            );

            return body;
        }

        protected virtual Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as ReferenceMapperContextTypeMapping;

            return Expression.Block
            (
                this.GetTargetInstanceAssignment( contextObj ),

                //assign to the object to return
                Expression.Assign( context.ReturnObjectVar, Expression.New(
                    context.ReturnTypeConstructor, context.SourcePropertyVar, context.TargetPropertyVar ) )
            );
        }

        private Expression GetTargetInstanceAssignment( object contextObj )
        {
            var context = contextObj as ReferenceMapperContextTypeMapping;
            var newInstanceExp = Expression.New( context.TargetPropertyType );

            //if( context.Mapping.TypeMapping.GlobalConfiguration.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
            //    return Expression.Assign( context.TargetPropertyVar, newInstanceExp );

            //var getValue = context.Mapping.TargetProperty.ValueGetter.Body.ReplaceParameter( context.TargetInstance );
            //return Expression.IfThenElse( Expression.Equal( getValue, context.TargetNullValue ),
            //        Expression.Assign( context.TargetPropertyVar, newInstanceExp ),
            //        Expression.Assign( context.TargetPropertyVar, getValue ) );

            return newInstanceExp;
        }
    }
}

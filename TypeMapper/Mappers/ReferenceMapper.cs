using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : IObjectMapperExpression
    {
#if DEBUG
        private static void debug( object o ) => Console.WriteLine( o );

        protected static readonly Expression<Action<object>> debugExp =
            ( o ) => debug( o );
#endif

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

        public virtual bool CanHandle( MemberMapping mapping )
        {
            var sourceType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetType = mapping.TargetProperty.MemberInfo.GetMemberType();

            return this.CanHandle( sourceType, targetType );
        }

        public virtual bool CanHandle( Type sourceType, Type targetType )
        {
            bool valueTypes = sourceType.IsValueType && targetType.IsValueType;
            bool builtInTypes = sourceType.IsBuiltInType( false ) && targetType.IsBuiltInType( false );

            return !valueTypes && !builtInTypes && !sourceType.IsEnumerable();
        }

        public LambdaExpression GetMappingExpression( MemberMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, ObjectPair>

            var context = this.GetMapperContext( mapping ) as ReferenceMapperContext;
            var expressionBody = this.GetExpressionBody( context );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceType, context.TargetType, context.ReturnType );

            return Expression.Lambda( delegateType, expressionBody,
                context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }

        protected virtual object GetMapperContext( MemberMapping mapping )
        {
            return new ReferenceMapperContext( mapping );
        }

        protected virtual Expression ReturnTypeInitialization( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;
            return Expression.Assign( context.ReturnObjectVar, Expression.Constant( null, context.ReturnType ) );
        }

        protected Expression GetExpressionBody( ReferenceMapperContext context )
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
                Expression.Assign( context.SourcePropertyVar, context.Mapping.SourceProperty.ValueGetter.Body
                    .ReplaceParameter(context.SourceInstance, context.Mapping.SourceProperty.ValueGetter.Parameters[ 0 ].Name ) ),

                Expression.IfThenElse
                (
                     Expression.Equal( context.SourcePropertyVar, context.SourceNullValue ),

                     Expression.Assign( context.TargetPropertyVar, context.TargetNullValue ),

                     Expression.Block
                     (
                        //object lookup
                        Expression.Assign( context.TargetPropertyVar, Expression.Convert(
                            Expression.Invoke( CacheLookupExpression, context.ReferenceTrack, context.SourcePropertyVar,
                            Expression.Constant( context.TargetPropertyType ) ), context.TargetPropertyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetPropertyVar, context.TargetNullValue ),
                            Expression.Block
                            (
                                this.GetInnerBody( context ),

                                //cache reference
                                Expression.Invoke( CacheAddExpression, context.ReferenceTrack, context.SourcePropertyVar,
                                    Expression.Constant( context.TargetPropertyType ), context.TargetPropertyVar )
                            )
                        )
                    )
                ),

                context.Mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( context.TargetInstance, context.Mapping.TargetProperty.ValueSetter.Parameters[ 0 ].Name )
                    .ReplaceParameter( context.TargetPropertyVar, context.Mapping.TargetProperty.ValueSetter.Parameters[ 1 ].Name ),

                context.ReturnObjectVar
            );

            return body;
        }

        protected virtual Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;

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
            var context = contextObj as ReferenceMapperContext;
            var newInstanceExp = Expression.New( context.TargetPropertyType );

            if( context.Mapping.TypeMapping.GlobalConfiguration.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                return Expression.Assign( context.TargetPropertyVar, newInstanceExp );

            var getValue = context.Mapping.TargetProperty.ValueGetter.Body.ReplaceParameter( context.TargetInstance );
            return Expression.IfThenElse( Expression.Equal( getValue, context.TargetNullValue ),
                    Expression.Assign( context.TargetPropertyVar, newInstanceExp ),
                    Expression.Assign( context.TargetPropertyVar, getValue ) );
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            throw new NotImplementedException();
        }
    }
}

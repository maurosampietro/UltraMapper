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

        protected static Func<ReferenceTracking, object, Type, object> refTrackingLookup =
         ( referenceTracker, sourceInstance, targetType ) =>
         {
             object targetInstance;
             referenceTracker.TryGetValue( sourceInstance, targetType, out targetInstance );

             return targetInstance;
         };

        protected static Action<ReferenceTracking, object, Type, object> addToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance, targetType, targetInstance );
        };

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
                typeof( ReferenceTracking ), context.SourceInstanceType, context.TargetInstanceType, context.ReturnType );

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
            return Expression.Assign( context.ReturnObject, Expression.Constant( null, context.ReturnType ) );
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

            Expression lookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, context.ReferenceTrack,
                context.SourceMember, Expression.Constant( context.TargetMemberType ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, context.ReferenceTrack, context.SourceMember,
                Expression.Constant( context.TargetMemberType ), context.TargetMember );

            var body = (Expression)Expression.Block
            (
                new ParameterExpression[] { context.SourceMember, context.TargetMember, context.ReturnObject },

                ReturnTypeInitialization( context ),

                //read source value
                Expression.Assign( context.SourceMember, context.SourceMemberValue ),

                Expression.IfThenElse
                (
                     Expression.Equal( context.SourceMember, context.SourceNullValue ),

                     Expression.Assign( context.TargetMember, context.TargetNullValue ),

                     Expression.Block
                     (
                        //object lookup
                        Expression.Assign( context.TargetMember,
                            Expression.Convert( lookupCall, context.TargetMemberType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetMember, context.TargetNullValue ),
                            Expression.Block
                            (
                                this.GetInnerBody( context ),

                                //cache reference
                                addToLookupCall
                            )
                        )
                    )
                ),

                context.Mapping.TargetProperty.ValueSetter.Body
                    .ReplaceParameter( context.TargetInstance, context.Mapping.TargetProperty.ValueSetter.Parameters[ 0 ].Name )
                    .ReplaceParameter( context.TargetMember, context.Mapping.TargetProperty.ValueSetter.Parameters[ 1 ].Name ),

                context.ReturnObject
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
                Expression.Assign( context.ReturnObject, Expression.New(
                    context.ReturnTypeConstructor, context.SourceMember, context.TargetMember ) )
            );
        }

        private Expression GetTargetInstanceAssignment( object contextObj )
        {
            var context = contextObj as ReferenceMapperContext;
            var newInstanceExp = Expression.New( context.TargetMemberType );

            if( context.Mapping.TypeMapping.GlobalConfiguration.ReferenceMappingStrategy == ReferenceMappingStrategies.CREATE_NEW_INSTANCE )
                return Expression.Assign( context.TargetMember, newInstanceExp );

            return Expression.IfThenElse( Expression.Equal( context.TargetMemberValue, context.TargetNullValue ),
                    Expression.Assign( context.TargetMember, newInstanceExp ),
                    Expression.Assign( context.TargetMember, context.TargetMemberValue ) );
        }
    }
}

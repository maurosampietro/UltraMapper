using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.TypeMappers
{
    public class ReferenceMapperTypeMapping : ReferenceMapper
    {
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

            var context = this.GetMapperContext( mapping ) as ReferenceMapperContext;
            var expressionBody = this.GetExpressionBody( context );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceInstanceType,
                context.TargetInstanceType, context.ReturnType );

            return Expression.Lambda( delegateType, expressionBody,
                context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }

        protected virtual object GetMapperContext( TypeMapping mapping )
        {
            return new ReferenceMapperContext( mapping );
        }

        protected override Expression GetExpressionBody( ReferenceMapperContext context )
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
                new ParameterExpression[] { context.SourceMember, context.TargetMember, context.ReturnObject },

                ReturnTypeInitialization( context ),

                //read source value
                Expression.Assign( context.SourceMember, context.SourceInstance ),

                Expression.IfThenElse
                (
                     Expression.Equal( context.SourceMember, context.SourceMemberNullValue ),

                     Expression.Assign( context.TargetMember, context.TargetMemberNullValue ),

                     Expression.Block
                     (
                        //object lookup
                        //Expression.Assign( context.TargetPropertyVar, Expression.Convert(
                        //    Expression.Invoke( CacheLookupExpression, context.ReferenceTrack, context.SourcePropertyVar,
                        //    Expression.Constant( context.TargetPropertyType ) ), context.TargetPropertyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetMember, context.TargetMemberNullValue ),
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

                context.ReturnObject
            );

            return body;
        }
    }
}

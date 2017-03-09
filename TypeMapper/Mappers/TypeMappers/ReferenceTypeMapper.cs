using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.TypeMappers
{
    public class ReferenceMapperTypeMapping : ReferenceMapper
    {
        public virtual bool CanHandle( TypeMapping mapping )
        {
            var sourceType = mapping.TypePair.SourceType;
            var targetType = mapping.TypePair.TargetType;

            return this.CanHandle( sourceType, targetType );
        }

        public LambdaExpression GetMappingExpression( TypeMapping mapping )
        {
            var context = this.GetMapperContext( mapping ) as ReferenceMapperContext;
            return this.GetMappingExpression( context );
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
                Expression.Assign( context.SourceMember, context.SourceInstance ),

                Expression.IfThenElse
                (
                     Expression.Equal( context.SourceMember, context.SourceMemberNullValue ),

                     Expression.Assign( context.TargetMember, context.TargetMemberNullValue ),

                     Expression.Block
                     (
                         //Il lookup incasina tutto perchè source e target sono inseriti in cache
                         //inizialmente, in questo caso operando sulle stessi oggetti di input
                         //direttamente vengono ritornate le istanze non ancora mappate.

                        ////object lookup
                        //Expression.Assign( context.TargetMember,
                        //    Expression.Convert( lookupCall, context.TargetMemberType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( context.TargetMember, context.TargetMemberNullValue ),
                            Expression.Block
                            (
                                this.GetInnerBody( context )
                                //,

                                //cache reference
                                //addToLookupCall
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

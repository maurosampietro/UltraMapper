using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapper : BaseReferenceObjectMapper, IObjectMapperExpression
    {
        public virtual bool CanHandle( PropertyMapping mapping )
        {
            bool valueTypes = !mapping.SourceProperty.PropertyInfo.PropertyType.IsValueType &&
                          !mapping.TargetProperty.PropertyInfo.PropertyType.IsValueType;

            return valueTypes && !mapping.TargetProperty.IsBuiltInType &&
                !mapping.SourceProperty.IsBuiltInType && !mapping.SourceProperty.IsEnumerable;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, ObjectPair>

            var context = this.GetMapperContext( mapping ) as ReferenceMapperContext;
            var expressionBody = this.GetExpressionBody( context );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), context.SourceType, context.TargetType, context.ReturnType );

            return Expression.Lambda( delegateType, expressionBody,
                context.ReferenceTrack, context.SourceInstance, context.TargetInstance );
        }

        protected virtual object GetMapperContext( PropertyMapping mapping )
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
                    .ReplaceParameter( context.SourceInstance ) ),

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
                    .ReplaceParameter( context.TargetInstance, "target" )
                    .ReplaceParameter( context.TargetPropertyVar, "value" ),

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
    }
}

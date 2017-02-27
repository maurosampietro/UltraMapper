using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Configuration;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class DictionaryMapper : CollectionMapper
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            bool sourceIsDictionary = typeof( IDictionary ).IsAssignableFrom(
                mapping.SourceProperty.MemberInfo.GetMemberType() );

            bool targetIsDictionary = typeof( IDictionary ).IsAssignableFrom(
                mapping.TargetProperty.MemberInfo.GetMemberType() );

            return sourceIsDictionary || targetIsDictionary;
        }

        protected override object GetMapperContext( MemberMapping mapping )
        {
            return new DictionaryMapperContext( mapping );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as DictionaryMapperContext;
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            bool keyIsBuiltInType = context.TargetCollectionElementKeyType.IsBuiltInType( false );
            bool valueIsBuiltInType = context.TargetCollectionElementValueType.IsBuiltInType( false );

            var addMethod = context.TargetMemberType.GetMethod( "Add" );

            var keyExpression = GetKeyOrValueExpression( context, context.SourceCollectionElementKey,
                context.TargetCollectionElementKey, context.SourceCollectionElementKeyType, context.TargetCollectionElementKeyType );

            var valueExpression = GetKeyOrValueExpression( context, context.SourceCollectionElementValue,
                context.TargetCollectionElementValue, context.SourceCollectionElementValueType, context.TargetCollectionElementValueType );

            return Expression.Block
            (
                new[] { context.TargetCollectionElementKey, context.TargetCollectionElementValue },

                Expression.Assign( context.TargetMember,
                    Expression.New( context.TargetMemberType ) ),

                ExpressionLoops.ForEach( context.SourceMember,
                    context.SourceCollectionLoopingVar, Expression.Block
                (
                    keyExpression,
                    valueExpression,

                    Expression.Call( context.TargetMember,
                        addMethod, context.TargetCollectionElementKey, context.TargetCollectionElementValue )
                ) )
            );
        }

        protected virtual Expression GetKeyOrValueExpression( DictionaryMapperContext context,
            MemberExpression sourceParam, ParameterExpression targetParam, Type sourceParamType, Type targetParamType )
        {
            bool isSourceKeyTypeBuiltIn = context.SourceCollectionElementKeyType.IsBuiltInType( false );
            bool isTargetKeyTypeBuiltIne = context.TargetCollectionElementKeyType.IsBuiltInType( false );

            if( sourceParamType.IsBuiltInType( false ) && targetParamType.IsBuiltInType( false ) )
            {
                if( sourceParamType == targetParamType )
                    return Expression.Assign( targetParam, sourceParam );

                var conversion = MappingExpressionBuilderFactory.GetMappingExpression(
                    sourceParamType, targetParamType );

                return Expression.Assign( targetParam, Expression.Invoke( conversion, sourceParam ) );
            }

            Expression lookupCall = Expression.Call( Expression.Constant( refTrackingLookup.Target ),
                refTrackingLookup.Method, context.ReferenceTrack, sourceParam,
                    Expression.Constant( targetParamType ) );

            Expression addToLookupCall = Expression.Call( Expression.Constant( addToTracker.Target ),
                addToTracker.Method, context.ReferenceTrack, sourceParam,
                Expression.Constant( targetParamType ), targetParam );

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();

            return Expression.Block
            (
                Expression.Assign( targetParam, Expression.Convert( lookupCall, targetParamType ) ),

                Expression.IfThen
                (
                    Expression.Equal( targetParam, Expression.Constant( null, targetParamType ) ),
                    Expression.Block
                    (
                        Expression.Assign( targetParam, Expression.New( targetParamType ) ),

                        //cache new collection
                        addToLookupCall,

                        //add to return list
                        Expression.Call( context.ReturnObject, addToRefCollectionMethod,
                            Expression.New( objectPairConstructor, sourceParam, targetParam ) )
                    )
                )
            );
        }
    }
}

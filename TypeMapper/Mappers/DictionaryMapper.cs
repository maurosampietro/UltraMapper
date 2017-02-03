using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        protected virtual Expression GetKeyOrValueExpression( DictionaryMapperContext context,
           MemberExpression sourceParam, ParameterExpression targetParam, Type sourceParamType, Type targetParamType )
        {
            bool isSourceKeyTypeBuiltIn = context.SourceKeyType.IsBuiltInType( false );
            bool isTargetKeyTypeBuiltIne = context.TargetKeyType.IsBuiltInType( false );

            if( sourceParamType.IsBuiltInType( false ) && targetParamType.IsBuiltInType( false ) )
            {
                if( sourceParamType == targetParamType )
                    return sourceParam;

                var typeMapping = context.Mapping.TypeMapping
                    .GlobalConfiguration.Configurator[ sourceParamType, targetParamType ];

                var convert = new BuiltInTypeMapper().GetMappingExpression( typeMapping );
                return Expression.Assign( targetParam, Expression.Invoke( convert, sourceParam ) );
            }

            var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var objectPairConstructor = context.ReturnElementType.GetConstructors().First();

            return Expression.Block
            (
                Expression.Assign( targetParam, Expression.Convert(
                    Expression.Invoke( CacheLookupExpression, context.ReferenceTrack, sourceParam,
                    Expression.Constant( sourceParamType ) ), targetParamType ) ),

                Expression.IfThen
                (
                    Expression.Equal( targetParam, Expression.Constant( null, targetParamType ) ),
                    Expression.Block
                    (
                        Expression.Assign( targetParam, Expression.New( targetParamType ) ),

                        //cache new collection
                        Expression.Invoke( CacheAddExpression, context.ReferenceTrack, sourceParam,
                            Expression.Constant( targetParamType ), targetParam ),

                        //add to return list
                        Expression.Call( context.ReturnObjectVar, addToRefCollectionMethod,
                            Expression.New( objectPairConstructor, sourceParam, targetParam ) )
                    )
                )
            );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as DictionaryMapperContext;
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            bool keyIsBuiltInType = context.TargetKeyType.IsBuiltInType( false );
            bool valueIsBuiltInType = context.TargetValueType.IsBuiltInType( false );

            var addMethod = context.TargetPropertyType.GetMethod( "Add" );

            var keyExpression = GetKeyOrValueExpression( context, context.SourceKey,
                context.TargetKey, context.SourceKeyType, context.TargetKeyType );

            var valueExpression = GetKeyOrValueExpression( context, context.SourceValue,
                context.TargetValue, context.SourceValueType, context.TargetValueType );

            return Expression.Block
            (
                new[] { context.TargetKey, context.TargetValue },

                Expression.Assign( context.TargetPropertyVar,
                    Expression.New( context.TargetPropertyType ) ),

                ExpressionLoops.ForEach( context.SourcePropertyVar, 
                    context.SourceLoopingVar, Expression.Block
                (
                    keyExpression,
                    valueExpression,

                    Expression.Call( context.TargetPropertyVar,
                        addMethod, context.TargetKey, context.TargetValue )
                ) )
            );
        }
    }
}

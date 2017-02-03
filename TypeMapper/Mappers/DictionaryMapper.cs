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
            return new CollectionMapperContext( mapping );
        }

        protected override Expression GetInnerBody( object contextObj )
        {
            var context = contextObj as CollectionMapperContext;
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            var sourceKey = Expression.Property( context.SourceLoopingVar, nameof( DictionaryEntry.Key ) );
            var sourceValue = Expression.Property( context.SourceLoopingVar, nameof( DictionaryEntry.Value ) );

            var sourceKeyType = context.SourceElementType.GetGenericArguments()[ 0 ];
            var sourceValueType = context.SourceElementType.GetGenericArguments()[ 1 ];

            var targetKeyType = context.TargetElementType.GetGenericArguments()[ 0 ];
            var targetValueType = context.TargetElementType.GetGenericArguments()[ 1 ];

            bool keyIsBuiltInType = targetKeyType.IsBuiltInType( false );
            bool valueIsBuiltInType = targetValueType.IsBuiltInType( false );

            var addMethod = context.TargetPropertyType.GetMethod( "Add" );

            Expression innerBody = null;
            if( keyIsBuiltInType && valueIsBuiltInType )
            {
                innerBody = ExpressionLoops.ForEach( context.SourcePropertyVar, context.SourceLoopingVar,
                    Expression.Call( context.TargetPropertyVar, addMethod, sourceKey, sourceValue ) );
            }
            else
            {
                var addToRefCollectionMethod = context.ReturnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var objectPairConstructor = context.ReturnElementType.GetConstructors().First();

                var targetKey = Expression.Variable( targetKeyType, "targetKey" );
                var targetValue = Expression.Variable( targetValueType, "targetValue" );

                Expression keyExpression = null;
                if( keyIsBuiltInType )
                    keyExpression = Expression.Assign( targetKey, sourceKey );
                else
                {
                    keyExpression = Expression.Block
                    (
                        Expression.Assign( targetKey, Expression.Convert( Expression.Invoke( CacheLookupExpression,
                          context.ReferenceTrack, sourceKey, Expression.Constant( sourceKeyType ) ), targetKeyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( targetKey, Expression.Constant( null, targetKeyType ) ),
                            Expression.Block
                            (
                                Expression.Assign( targetKey, Expression.New( targetKeyType ) ),

                                //cache new collection
                                Expression.Invoke( CacheAddExpression, context.ReferenceTrack, sourceKey,
                                    Expression.Constant( targetKeyType ), targetKey ),

                                //add to return list
                                Expression.Call( context.ReturnObjectVar, addToRefCollectionMethod,
                                    Expression.New( objectPairConstructor, sourceKey, targetKey ) )
                            )
                        )
                    );
                }

                Expression valueExpression = null;
                if( valueIsBuiltInType )
                    valueExpression = Expression.Assign( targetValue, sourceValue );
                else
                {
                    valueExpression = Expression.Block
                    (
                        Expression.Assign( targetValue, Expression.Convert( Expression.Invoke( CacheLookupExpression,
                          context.ReferenceTrack, sourceValue, Expression.Constant( sourceValueType ) ), targetValueType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( targetValue, Expression.Constant( null, targetValueType ) ),
                            Expression.Block
                            (
                                Expression.Assign( targetValue, Expression.New( targetValueType ) ),

                                //cache new collection
                                Expression.Invoke( CacheAddExpression, context.ReferenceTrack, sourceValue,
                                    Expression.Constant( targetValueType ), targetValue ),

                                //add to return list
                                Expression.Call( context.ReturnObjectVar, addToRefCollectionMethod,
                                    Expression.New( objectPairConstructor, sourceValue, targetValue ) )
                            )
                        )
                    );
                }

                innerBody = Expression.Block
                (
                    new[] { targetKey, targetValue },

                    ExpressionLoops.ForEach( context.SourcePropertyVar, context.SourceLoopingVar, Expression.Block
                    (
                        keyExpression,
                        valueExpression,
                        Expression.Call( context.TargetPropertyVar, addMethod, targetKey, targetValue )
                    ) )
                );
            }

            return Expression.Block
            (
                Expression.Assign( context.TargetPropertyVar,
                    Expression.New( context.TargetPropertyType ) ),

                innerBody
            );
        }    
    }
}

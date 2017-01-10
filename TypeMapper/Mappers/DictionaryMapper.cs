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
    public class DictionaryMapper : BaseReferenceObjectMapper, IObjectMapperExpression
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            bool sourceIsDictionary = typeof( IDictionary ).IsAssignableFrom(
                mapping.SourceProperty.PropertyInfo.PropertyType );

            bool targetIsDictionary = typeof( IDictionary ).IsAssignableFrom(
                mapping.TargetProperty.PropertyInfo.PropertyType );

            return sourceIsDictionary || targetIsDictionary;
        }

        public LambdaExpression GetMappingExpression( PropertyMapping mapping )
        {
            //Func<ReferenceTracking, sourceType, targetType, IEnumerable<ObjectPair>>

            var returnType = typeof( List<ObjectPair> );
            var returnElementType = typeof( ObjectPair );

            var sourceType = mapping.SourceProperty.PropertyInfo.ReflectedType;
            var targetType = mapping.TargetProperty.PropertyInfo.ReflectedType;

            var sourceCollectionType = mapping.SourceProperty.PropertyInfo.PropertyType;
            var targetCollectionType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceElementType = sourceCollectionType.GetCollectionGenericType();
            var targetElementType = targetCollectionType.GetCollectionGenericType();

            bool isSourceElementTypeBuiltIn = sourceCollectionType.IsBuiltInType( false );
            bool isTargetElementTypeBuiltIn = targetElementType.IsBuiltInType( false );

            var addMethod = targetCollectionType.GetMethod( "Add" );

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            var sourceCollection = Expression.Variable( sourceCollectionType, "sourceCollection" );
            var targetCollection = Expression.Variable( targetCollectionType, "targetCollection" );

            var nullExp = Expression.Constant( null, sourceCollectionType );
            var sourceLoopVar = Expression.Parameter( sourceElementType, "loopVar" );
            var newRefObjects = Expression.Variable( returnType, "result" );

            var sourceKey = Expression.Property( sourceLoopVar, nameof( DictionaryEntry.Key ) );
            var sourceValue = Expression.Property( sourceLoopVar, nameof( DictionaryEntry.Value ) );

            var sourceKeyType = sourceElementType.GetGenericArguments()[ 0 ];
            var sourceValueType = sourceElementType.GetGenericArguments()[ 1 ];

            var targetKeyType = targetElementType.GetGenericArguments()[ 0 ];
            var targetValueType = targetElementType.GetGenericArguments()[ 1 ];

            bool keyIsBuiltInType = targetKeyType.IsBuiltInType( false );
            bool valueIsBuiltInType = targetValueType.IsBuiltInType( false );

            Expression innerBody = null;
            if( keyIsBuiltInType && valueIsBuiltInType )
            {
                innerBody = ExpressionLoops.ForEach( sourceCollection, sourceLoopVar,
                    Expression.Call( targetCollection, addMethod, sourceKey, sourceValue ) );
            }
            else
            {
                var addToRefCollectionMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var objectPairConstructor = returnElementType.GetConstructors().First();

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
                            referenceTrack, sourceKey, Expression.Constant( sourceKeyType ) ), targetKeyType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( targetKey, Expression.Constant( null, targetKeyType ) ),
                            Expression.Block
                            (
                                Expression.Assign( targetKey, Expression.New( targetKeyType ) ),

                                //cache new collection
                                Expression.Invoke( CacheAddExpression, referenceTrack, sourceKey,
                                    Expression.Constant( targetKeyType ), targetKey ),

                                //add to return list
                                Expression.Call( newRefObjects, addToRefCollectionMethod,
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
                           referenceTrack, sourceValue, Expression.Constant( sourceValueType ) ), targetValueType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( targetValue, Expression.Constant( null, targetValueType ) ),
                            Expression.Block
                            (
                                Expression.Assign( targetValue, Expression.New( targetValueType ) ),

                                //cache new collection
                                Expression.Invoke( CacheAddExpression, referenceTrack, sourceValue,
                                    Expression.Constant( targetValueType ), targetValue ),

                                //add to return list
                                Expression.Call( newRefObjects, addToRefCollectionMethod,
                                    Expression.New( objectPairConstructor, sourceValue, targetValue ) )
                            )
                        )
                    );
                }

                innerBody = Expression.Block
                (
                    new[] { targetKey, targetValue },

                    ExpressionLoops.ForEach( sourceCollection, sourceLoopVar, Expression.Block
                    (
                        keyExpression,
                        valueExpression,
                        Expression.Call( targetCollection, addMethod, targetKey, targetValue )
                    ) )
                );
            }

            var body = Expression.Block
            (
                new[] { sourceCollection, targetCollection, newRefObjects },

                Expression.Assign( newRefObjects, Expression.New( returnType ) ),
                Expression.Assign( sourceCollection, mapping.SourceProperty.ValueGetter.Body ).ReplaceParameter( sourceInstance, "target" ),

                Expression.IfThenElse
                (
                    Expression.Equal( sourceCollection, nullExp ),

                    mapping.TargetProperty.ValueSetter.Body
                        .ReplaceParameter( targetInstance, "target" )
                        .ReplaceParameter( targetCollection, "value" ),

                    Expression.Block
                    (
                        Expression.Assign( targetCollection, Expression.Convert( Expression.Invoke( CacheLookupExpression,
                            referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ) ), targetCollectionType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( targetCollection, Expression.Constant( null, targetCollectionType ) ),
                            Expression.Block
                            (
                                Expression.Assign( targetCollection, Expression.New( targetCollectionType ) ),

                                innerBody,

                                //cache new collection
                                Expression.Invoke( CacheAddExpression, referenceTrack, sourceCollection,
                                    Expression.Constant( targetCollectionType ), targetCollection )
                            )
                        ),

                        mapping.TargetProperty.ValueSetter.Body
                            .ReplaceParameter( targetInstance, "target" )
                            .ReplaceParameter( targetCollection, "value" )
                    )
                ),

                newRefObjects
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType, body, 
                referenceTrack, sourceInstance, targetInstance );
        }
    }
}

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
    public class DictionaryMapper : IObjectMapperExpression
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

        private static Expression<Func<ReferenceTracking, object, Type, object>> lookup =
             ( rT, sI, tT ) => refTrackingLookup( rT, sI, tT );

        private static Expression<Action<ReferenceTracking, object, Type, object>> add =
            ( rT, sI, tT, tI ) => addToTracker( rT, sI, tT, tI );

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

            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

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
            var loopVar = Expression.Parameter( sourceElementType, "loopVar" );
            var newRefObjects = Expression.Variable( returnType, "result" );

            var keyProperty = Expression.Property( loopVar, nameof( DictionaryEntry.Key ) );
            var valueProperty = Expression.Property( loopVar, nameof( DictionaryEntry.Value ) );

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
                        Expression.Assign( targetCollection, Expression.Convert( Expression.Invoke( lookup,
                            referenceTrack, sourceCollection, Expression.Constant( targetCollectionType ) ), targetCollectionType ) ),

                        Expression.IfThen
                        (
                            Expression.Equal( targetCollection, Expression.Constant( null, targetCollectionType ) ),
                            Expression.Block
                            (
                                Expression.Assign( targetCollection, Expression.New( targetCollectionType ) ),

                                ExpressionLoops.ForEach( sourceCollection, loopVar, Expression.Block
                                (
                                    Expression.Call( targetCollection, addMethod, keyProperty, valueProperty )
                                ) )
                            )
                        )
                    )
                ),

                newRefObjects
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                typeof( ReferenceTracking ), sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, referenceTrack, sourceInstance, targetInstance );
        }

        public IEnumerable<ObjectPair> Map( object source, object targetInstance,
            PropertyMapping mapping, IReferenceTracking referenceTracking )
        {
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            object trackedCollection;
            if( referenceTracking.TryGetValue( source, targetPropertyType, out trackedCollection ) )
            {
                //mapping.TargetProperty.ValueSetter( targetInstance, trackedCollection );
                yield break;
            }

            //map 'the container' itself
            var collection = mapping.TargetProperty.CollectionStrategy
                 .GetTargetCollection<IDictionary>( targetInstance, mapping );

            referenceTracking.Add( source, targetPropertyType, collection );
            //mapping.TargetProperty.ValueSetter( targetInstance, collection );

            //map contained items
            Type genericType = targetPropertyType.GetCollectionGenericType();
            var keyType = genericType.GetGenericArguments()[ 0 ];
            var valueType = genericType.GetGenericArguments()[ 1 ];

            bool keyIsBuiltInType = keyType.IsBuiltInType( false );
            bool valueIsBuiltInType = keyType.IsBuiltInType( false );

            //var keyProp = genericType.GetProperty( "Key" ).BuildUntypedCastGetter();
            //var valueProp = genericType.GetProperty( "Value" ).BuildUntypedCastGetter();

            if( keyIsBuiltInType && valueIsBuiltInType )
            {
                foreach( DictionaryEntry sourceItem in (IDictionary)source )
                    collection.Add( sourceItem.Key, sourceItem.Value );
            }
            else if( !keyIsBuiltInType && !valueIsBuiltInType )
            {
                foreach( DictionaryEntry sourceItem in (IDictionary)source )
                {
                    var key = sourceItem.Key;// keyProp( sourceItem );
                    var value = sourceItem.Value;// valueProp( sourceItem );

                    object targetItemKey;
                    if( !referenceTracking.TryGetValue( key, keyType, out targetItemKey ) )
                    {
                        targetItemKey = Activator.CreateInstance( keyType );

                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                        referenceTracking.Add( key, keyType, targetItemKey );
                        yield return new ObjectPair( key, targetItemKey );
                    }

                    object targetItemValue;
                    if( !referenceTracking.TryGetValue( value, valueType, out targetItemValue ) )
                    {
                        targetItemValue = Activator.CreateInstance( valueType );

                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                        referenceTracking.Add( value, valueType, targetItemKey );
                        yield return new ObjectPair( value, targetItemValue );
                    }

                    collection.Add( targetItemKey, targetItemValue );
                }
            }
            else
            {
                foreach( DictionaryEntry sourceItem in (IDictionary)source )
                {
                    var key = sourceItem.Key;// keyProp( sourceItem );
                    var value = sourceItem.Value;// valueProp( sourceItem );

                    object targetItemKey, targetItemValue;
                    if( keyIsBuiltInType )
                    {
                        targetItemKey = key;
                    }
                    else if( !referenceTracking.TryGetValue( key, keyType, out targetItemKey ) )
                    {
                        targetItemKey = Activator.CreateInstance( keyType );

                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                        referenceTracking.Add( key, keyType, targetItemKey );
                        yield return new ObjectPair( key, targetItemKey );
                    }


                    if( valueIsBuiltInType )
                    {
                        targetItemValue = value;
                    }
                    else if( !referenceTracking.TryGetValue( value, valueType, out targetItemValue ) )
                    {
                        targetItemValue = Activator.CreateInstance( valueType );

                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                        referenceTracking.Add( value, valueType, targetItemKey );
                        yield return new ObjectPair( value, targetItemValue );
                    }

                    collection.Add( targetItemKey, targetItemValue );
                }
            }
        }
    }
}

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using TypeMapper.Internals;

//namespace TypeMapper.Mappers
//{
//    public class DictionaryMapper : IObjectMapperExpression
//    {
//        public bool CanHandle( PropertyMapping mapping )
//        {
//            bool sourceIsDictionary = typeof( IDictionary ).IsAssignableFrom(
//                mapping.SourceProperty.PropertyInfo.PropertyType );

//            bool targetIsDictionary = typeof( IDictionary ).IsAssignableFrom(
//                mapping.TargetProperty.PropertyInfo.PropertyType );

//            return sourceIsDictionary || targetIsDictionary;
//        }

//        public IEnumerable<ObjectPair> Map( object source, object targetInstance,
//            PropertyMapping mapping, IReferenceTracking referenceTracking )
//        {
//            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

//            object trackedCollection;
//            if( referenceTracking.TryGetValue( source, targetPropertyType, out trackedCollection ) )
//            {
//                //mapping.TargetProperty.ValueSetter( targetInstance, trackedCollection );
//                yield break;
//            }

//            //map 'the container' itself
//            var collection = mapping.TargetProperty.CollectionStrategy
//                 .GetTargetCollection<IDictionary>( targetInstance, mapping );

//            referenceTracking.Add( source, targetPropertyType, collection );
//            //mapping.TargetProperty.ValueSetter( targetInstance, collection );

//            //map contained items
//            Type genericType = targetPropertyType.GetCollectionGenericType();
//            var keyType = genericType.GetGenericArguments()[ 0 ];
//            var valueType = genericType.GetGenericArguments()[ 1 ];

//            bool keyIsBuiltInType = keyType.IsBuiltInType( false );
//            bool valueIsBuiltInType = keyType.IsBuiltInType( false );

//            //var keyProp = genericType.GetProperty( "Key" ).BuildUntypedCastGetter();
//            //var valueProp = genericType.GetProperty( "Value" ).BuildUntypedCastGetter();

//            if( keyIsBuiltInType && valueIsBuiltInType )
//            {
//                foreach( DictionaryEntry sourceItem in (IDictionary)source )
//                    collection.Add( sourceItem.Key, sourceItem.Value );
//            }
//            else if( !keyIsBuiltInType && !valueIsBuiltInType )
//            {
//                foreach( DictionaryEntry sourceItem in (IDictionary)source )
//                {
//                    var key = sourceItem.Key;// keyProp( sourceItem );
//                    var value = sourceItem.Value;// valueProp( sourceItem );

//                    object targetItemKey;
//                    if( !referenceTracking.TryGetValue( key, keyType, out targetItemKey ) )
//                    {
//                        targetItemKey = Activator.CreateInstance( keyType );

//                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
//                        referenceTracking.Add( key, keyType, targetItemKey );
//                        yield return new ObjectPair( key, targetItemKey );
//                    }

//                    object targetItemValue;
//                    if( !referenceTracking.TryGetValue( value, valueType, out targetItemValue ) )
//                    {
//                        targetItemValue = Activator.CreateInstance( valueType );

//                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
//                        referenceTracking.Add( value, valueType, targetItemKey );
//                        yield return new ObjectPair( value, targetItemValue );
//                    }

//                    collection.Add( targetItemKey, targetItemValue );
//                }
//            }
//            else
//            {
//                foreach( DictionaryEntry sourceItem in (IDictionary)source )
//                {
//                    var key = sourceItem.Key;// keyProp( sourceItem );
//                    var value = sourceItem.Value;// valueProp( sourceItem );

//                    object targetItemKey, targetItemValue;
//                    if( keyIsBuiltInType )
//                    {
//                        targetItemKey = key;
//                    }
//                    else if( !referenceTracking.TryGetValue( key, keyType, out targetItemKey ) )
//                    {
//                        targetItemKey = Activator.CreateInstance( keyType );

//                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
//                        referenceTracking.Add( key, keyType, targetItemKey );
//                        yield return new ObjectPair( key, targetItemKey );
//                    }


//                    if( valueIsBuiltInType )
//                    {
//                        targetItemValue = value;
//                    }
//                    else if( !referenceTracking.TryGetValue( value, valueType, out targetItemValue ) )
//                    {
//                        targetItemValue = Activator.CreateInstance( valueType );

//                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
//                        referenceTracking.Add( value, valueType, targetItemKey );
//                        yield return new ObjectPair( value, targetItemValue );
//                    }

//                    collection.Add( targetItemKey, targetItemValue );
//                }
//            }
//        }
//    }
//}

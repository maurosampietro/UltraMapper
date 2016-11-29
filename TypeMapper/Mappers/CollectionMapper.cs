using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class CollectionMapper : IObjectMapper
    {
        public bool CanHandle( PropertyMapping mapping )
        {
            //the following check avoids to treat a string as a collection
            return mapping.SourceProperty.IsEnumerable &&
                !mapping.SourceProperty.IsBuiltInType;
        }

        public IEnumerable<ObjectPair> Map( object source, object targetInstance,
            PropertyMapping mapping, IReferenceTracking referenceTracking )
        {
            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            object trackedCollection;
            if( referenceTracking.TryGetValue( source, targetPropertyType, out trackedCollection ) )
            {
                mapping.TargetProperty.ValueSetter( targetInstance, trackedCollection );
                yield break;
            }

            //map 'the container' itself
            var collection = mapping.TargetProperty.CollectionStrategy
                .GetTargetCollection<IList>( targetInstance, mapping );

            referenceTracking.Add( source, targetPropertyType, targetInstance );
            mapping.TargetProperty.ValueSetter( targetInstance, collection );

            //map contained items
            Type genericType = targetPropertyType.GetCollectionGenericType();
            bool isBuiltInType = genericType.IsBuiltInType( false );

            foreach( var sourceItem in (IEnumerable)source )
            {
                object targetItem;
                if( isBuiltInType )
                {
                    targetItem = sourceItem;
                }
                else
                {
                    if( !referenceTracking.TryGetValue( sourceItem,
                        genericType, out targetItem ) )
                    {
                        targetItem = Activator.CreateInstance( genericType );

                        //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                        referenceTracking.Add( sourceItem, genericType, targetItem );
                        yield return new ObjectPair( sourceItem, targetItem );
                    }
                }

                collection.Add( targetItem );
            }
        }
    }
}

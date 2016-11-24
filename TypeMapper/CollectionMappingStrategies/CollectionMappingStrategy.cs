using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.CollectionMappingStrategies
{
    public interface ICollectionMapper
    {
        void AddItem( IList collection, object item );
    }

    /// <summary>
    /// Creates a new collection and assign it to the input collection
    /// </summary>
    public class ReplaceCollection : ICollectionMappingStrategy
    {
        public void AddItem( IList collection, object item )
        {
            collection.Add( item );
        }
    }

    /// <summary>
    /// Keeps using the input collection and maps
    /// removing and adding element to it.
    /// </summary>
    public class UpdateCollection : ICollectionMappingStrategy
    {

    }

    /// <summary>
    /// Keeps the input collection and adds elements to it
    /// </summary>
    public class MergeCollection : ICollectionMappingStrategy
    {

    }
}

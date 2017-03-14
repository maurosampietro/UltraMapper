using System;

namespace TypeMapper.CollectionMappingStrategies
{
    /// <summary>
    /// Keeps using the input collection (same reference).
    /// The collection is cleared and then elements are added.
    /// </summary>
    public class ClearCollection : ICollectionMappingStrategy
    {
       
    }

    /// <summary>
    /// Keeps using the input collection (same reference).
    /// Each source item matching a target item is updated.
    /// Each source item non existing in the target collection is added.
    /// Each target item non existing in the source collection is removed.
    /// </summary>
    public class UpdateCollection : ICollectionMappingStrategy
    {
      
    }

    /// <summary>
    /// Keep using the input collection (same reference).
    /// The collection is untouched and elements are added.
    /// </summary>
    public class MergeCollection : ICollectionMappingStrategy
    {
      
    }
}

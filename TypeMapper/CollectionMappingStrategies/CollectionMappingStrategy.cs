using System;

namespace TypeMapper.CollectionMappingStrategies
{
    ///// <summary>
    ///// Creates a new collection
    ///// </summary>
    //public class NewCollection : ICollectionMappingStrategy
    //{
    //    public TReturn GetTargetCollection<TReturn>( object targetInstance, PropertyMapping mapping )
    //    {
    //        return (TReturn)Activator.CreateInstance( mapping.TargetProperty.MemberInfo.PropertyType );
    //    }
    //}

    /// <summary>
    /// If a collection already exists on the target, keeps using it.
    /// A new collection is created otherwise.
    /// </summary>
    //public class KeepCollection : ICollectionMappingStrategy
    //{
    //    public TReturn GetTargetCollection<TReturn>( object targetInstance, PropertyMapping mapping )
    //    {
    //        var targetProperty = mapping.TargetProperty.MemberInfo;

    //        object collection = targetProperty.GetValue( targetInstance );
    //        if( collection == null)
    //            collection = Activator.CreateInstance( targetProperty.PropertyType );

    //        return (TReturn)collection;
    //    }
    //}

    /// <summary>
    /// Keeps using the input collection and maps
    /// removing and adding element to it.
    /// </summary>
    //public class UpdateCollection : ICollectionMappingStrategy
    //{
    //    public IList GetTargetCollection( object targetInstance, PropertyMapping mapping )
    //    {
    //        return (IList)mapping.TargetProperty.PropertyInfo.GetValue( targetInstance );
    //    }
    //}

    ///// <summary>
    ///// Keeps the input collection and adds elements to it
    ///// </summary>
    //public class MergeCollection : ICollectionMappingStrategy
    //{
    //    public IList GetTargetCollection( object targetInstance, PropertyMapping mapping )
    //    {


    //        return (IList)mapping.TargetProperty.PropertyInfo.GetValue( targetInstance );
    //    }
    //}
}

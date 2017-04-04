using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Mappers;

namespace UltraMapper
{
    public enum CollectionMappingStrategies { MERGE, RESET, UPDATE }

    ///// <summary>
    ///// Keeps using the input collection (same reference).
    ///// The collection is cleared and then elements are added.
    ///// </summary>
    //public class ClearAndAddToCollection : ICollectionMappingStrategy
    //{
    //    public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
    //    {
    //        var clearMethod = GetTargetCollectionClearMethod( context );
    //        return Expression.Block
    //        (
    //            Expression.Call( context.TargetInstance, clearMethod )
    //        //,
    //        //SimpleCollectionLoop( context, context.SourceInstance, context.TargetInstance )
    //        );
    //    }

    //    public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
    //    {
    //        var clearMethod = GetTargetCollectionClearMethod( context );
    //        return Expression.Block
    //        (
    //            Expression.Call( context.TargetInstance, clearMethod )
    //        //,
    //        //CollectionLoopWithReferenceTracking( context, context.SourceInstance, context.TargetInstance )
    //        );
    //    }

    //    /// <summary>
    //    /// Returns the method that allows to clear the target collection.
    //    /// </summary>
    //    private MethodInfo GetTargetCollectionClearMethod( CollectionMapperContext context )
    //    {
    //        //It is forbidden to use nameof with unbound generic types. We use 'int' just to get around that.
    //        var clearMethod = context.TargetInstance.Type.GetMethod( nameof( ICollection<int>.Clear ) );

    //        if( clearMethod == null )
    //        {
    //            string msg = $@"Cannot map to type '{nameof( context.TargetInstance.Type )}' does not provide a clear method";
    //            throw new Exception( msg );
    //        }

    //        return clearMethod;
    //    }
    //}

    ///// <summary>
    ///// Keeps using the input collection (same reference).
    ///// Each source item matching a target item is updated.
    ///// Each source item non existing in the target collection is added.
    ///// Each target item non existing in the source collection is removed.
    ///// </summary>
    //public class UpdateCollection : ICollectionMappingStrategy
    //{
    //    public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    ///// <summary>
    ///// Keep using the input collection (same reference).
    ///// The collection is untouched and elements are added.
    ///// </summary>
    //public class AddToCollection : ICollectionMappingStrategy
    //{
    //    public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}

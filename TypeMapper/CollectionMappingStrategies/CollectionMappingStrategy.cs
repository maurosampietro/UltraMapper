using System;
using System.Linq.Expressions;
using TypeMapper.Mappers;

namespace TypeMapper.CollectionMappingStrategies
{
    /// <summary>
    /// Keeps using the input collection (same reference).
    /// The collection is cleared and then elements are added.
    /// </summary>
    public class ClearCollection : ICollectionMappingStrategy
    {
        public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }

        public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Keeps using the input collection (same reference).
    /// Each source item matching a target item is updated.
    /// Each source item non existing in the target collection is added.
    /// Each target item non existing in the source collection is removed.
    /// </summary>
    public class UpdateCollection : ICollectionMappingStrategy
    {
        public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }

        public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Keep using the input collection (same reference).
    /// The collection is untouched and elements are added.
    /// </summary>
    public class MergeCollection : ICollectionMappingStrategy
    {
        public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }

        public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }
    }

    public class NewCollection : ICollectionMappingStrategy
    {
        //public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        //{
        //    var constructor = GetTargetCollectionConstructorFromCollection( context );
        //    var targetCollectionConstructor = Expression.New( constructor, context.SourceMember );

        //    return Expression.Assign( context.TargetMember, targetCollectionConstructor );
        //}

        //public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        //{
        //    throw new NotImplementedException();
        //}
        public Expression GetComplexTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }

        public Expression GetSimpleTypeInnerBody( CollectionMapperContext context )
        {
            throw new NotImplementedException();
        }
    }
}

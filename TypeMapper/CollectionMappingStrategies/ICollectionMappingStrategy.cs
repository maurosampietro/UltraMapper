using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers;

namespace TypeMapper.CollectionMappingStrategies
{
    /// <summary>
    /// The strategy to adopt to deal with a target collection when
    /// ReferenceMappingStrategy == USE_TARGET_INSTANCE_IF_NOT_NULL is used.
    /// </summary>
    public interface ICollectionMappingStrategy
    {
        Expression GetSimpleTypeInnerBody( CollectionMapperContext context );
        Expression GetComplexTypeInnerBody( CollectionMapperContext context );
    }
}
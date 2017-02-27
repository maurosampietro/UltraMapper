using TypeMapper.Internals;

namespace TypeMapper.CollectionMappingStrategies
{
    public interface ICollectionMappingStrategy
    {
        TReturn GetTargetCollection<TReturn>( object targetInstance, MemberMapping mapping );
    }
}
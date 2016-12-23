using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Configuration;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class GlobalConfiguration
    {
        /// <summary>
        /// If set to True only explicit user-defined mappings are used.
        /// If set to False mappings are generated based on conventions
        /// and the user can override them.
        /// </summary>
        public bool IgnoreConventions { get; set; }

        public ICollectionMappingStrategy CollectionMappingStrategy { get; set; }
        public ReferenceMappingStrategies ReferenceMappingStrategy { get; set; }

        public IMappingConvention MappingConvention { get; set; }
        public ObjectMapperSet Mappers { get; set; }
    }

    public enum ReferenceMappingStrategies { CREATE_NEW_INSTANCE, USE_TARGET_INSTANCE_IF_NOT_NULL }

}
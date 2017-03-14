using System.Collections.Generic;
using System.Linq.Expressions;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Configuration;
using TypeMapper.Mappers;
using TypeMapper.Mappers.TypeMappers;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public interface IMemberOptions
    {
        ICollectionMappingStrategy CollectionMappingStrategy { get; }
        ReferenceMappingStrategies ReferenceMappingStrategy { get; }
        LambdaExpression CustomConverter { get; }
        LambdaExpression CustomTargetConstructor { get; }
    }

    public interface ITypeOptions : IMemberOptions
    {
        bool IgnoreMappingResolvedByConvention { get; }
    }

    public class GlobalConfiguration
    {
        public readonly MapperConfiguration Configurator;

        /// <summary>
        /// If set to True only explicit user-defined mappings are used.
        /// If set to False mappings are generated based on conventions
        /// and the user can override them.
        /// </summary>
        public bool IgnoreMappingResolvedByConvention { get; set; }

        public ICollectionMappingStrategy CollectionMappingStrategy { get; set; }
        public ReferenceMappingStrategies ReferenceMappingStrategy { get; set; }

        public IMappingConvention MappingConvention { get; set; }
        public HashSet<IMemberMappingMapperExpression> Mappers { get; private set; }

        public GlobalConfiguration( MapperConfiguration configurator )
        {
            this.Configurator = configurator;
            this.Mappers = new HashSet<IMemberMappingMapperExpression>()
            {
                //order is important: the first mapper that can handle a mapping is used
                new CustomConverterMapper(),
                new BuiltInTypeMapper(),
                new NullableMapper(),
                new ConvertMapper(),
                new StructMapper(),
                new ReferenceMapper(),
                new DictionaryMapper(),
                new StackMapper(),
                new QueueMapper(),
                new LinkedListMapper(),
                new CollectionMapper(),
                new CollectionMapperTypeMapping(),
            };
        }
    }

    public enum ReferenceMappingStrategies { CREATE_NEW_INSTANCE, USE_TARGET_INSTANCE_IF_NOT_NULL }
}
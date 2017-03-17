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
        public HashSet<IMapperExpressionBuilder> Mappers { get; private set; }

        public GlobalConfiguration( MapperConfiguration configurator )
        {
            this.Configurator = configurator;
            this.Mappers = new HashSet<IMapperExpressionBuilder>()
            {
                //order is important: the first mapper that can handle a mapping is used
                new BuiltInTypeMapper(this),
                new NullableMapper(this),
                new ConvertMapper(this),
                new StructMapper(this),
                new ReferenceMapper(this),
                new DictionaryMapper(this),
                new StackMapper(this),
                new QueueMapper(this),
                new LinkedListMapper(this),
                new CollectionMapper(this),
                new CollectionMapperTypeMapping(this),
            };
        }
    }

    public enum ReferenceMappingStrategies { CREATE_NEW_INSTANCE, USE_TARGET_INSTANCE_IF_NOT_NULL }
}
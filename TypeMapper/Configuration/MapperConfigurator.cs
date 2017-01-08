using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class MapperConfiguration<T> : MapperConfiguration where T : IMappingConvention, new()
    {
        public MapperConfiguration() { }

        public MapperConfiguration(Action<T> config)
              : base(new T()) { config?.Invoke((T)GlobalConfiguration.MappingConvention); }
    }


    public class MapperConfiguration
    {
        protected readonly Dictionary<TypePair, TypeMapping> _typeMappings =
            new Dictionary<TypePair, TypeMapping>();

        public readonly GlobalConfiguration GlobalConfiguration;

        public MapperConfiguration(IMappingConvention mappingConvention)
            : this()
        {
            GlobalConfiguration.MappingConvention = mappingConvention;
        }

        public MapperConfiguration(Action<DefaultMappingConvention> config)
             : this()
        {
            config?.Invoke((DefaultMappingConvention)GlobalConfiguration.MappingConvention);
        }

        public MapperConfiguration()
        {
            GlobalConfiguration = new GlobalConfiguration(this)
            {
                MappingConvention = new DefaultMappingConvention(),

                //order is important: the first mapper that can handle a mapping is used
                Mappers = new ObjectMapperSet()
                    .Add<CustomConverterMapper>()
                    .Add<BuiltInTypeMapper>()
                    .Add<NullableMapper>()
                    .Add<ConvertMapper>()
                    .Add<ReferenceMapper>()
                    .Add<DictionaryMapper>()
                    .Add<SetMapper>()
                    .Add<StackMapper>()
                    //.Add<QueueMapper>()
                    .Add<LinkedListMapper>()
                    .Add<CollectionMapper>()
            };
        }

        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            Action<TypeMapping> configTypeMapping = null)
        {
            var typeMapping = this.GetTypeMapping(typeof(TSource), typeof(TTarget));
            configTypeMapping?.Invoke(typeMapping);

            return new TypeMappingConfigurator<TSource, TTarget>(typeMapping, GlobalConfiguration);
        }

        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            TSource source, TTarget target, Action<TypeMapping> configTypeMapping = null)
        {
            var typeMapping = this.GetTypeMapping(source.GetType(), target.GetType());
            configTypeMapping?.Invoke(typeMapping);

            return new TypeMappingConfigurator<TSource, TTarget>(typeMapping, GlobalConfiguration);
        }

        private TypeMapping GetTypeMapping(Type sourceType, Type targetType)
        {
            var typePair = new TypePair(sourceType, targetType);

            TypeMapping typeMapping;
            if (!_typeMappings.TryGetValue(typePair, out typeMapping))
            {
                typeMapping = new TypeMapping(GlobalConfiguration, typePair);
                _typeMappings.Add(typePair, typeMapping);
            }

            return typeMapping;
        }

        public TypeMapping this[Type source, Type target]
        {
            get
            {
                var typePair = new TypePair(source, target);

                TypeMapping typeMapping;
                if (_typeMappings.TryGetValue(typePair, out typeMapping))
                    return typeMapping;

                if (GlobalConfiguration.IgnoreConventions)
                    throw new Exception($"Cannot handle {typePair}. No mapping have been explicitly defined for '{typePair}' and mapping by convention has been disabled.");

                typeMapping = new TypeMapping(GlobalConfiguration, typePair);
                new TypeMappingConfigurator(typeMapping, GlobalConfiguration)
                    .MapByConvention(typeMapping);

                _typeMappings.Add(typePair, typeMapping);
                return typeMapping;
            }
        }
    }
}

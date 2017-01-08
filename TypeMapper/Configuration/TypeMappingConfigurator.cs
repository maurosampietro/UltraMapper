using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Configuration
{
    public class TypeMappingConfigurator
    {
        //Each source and target property can be instantiated only once per configuration
        //so we can handle their options correctly.
        protected readonly Dictionary<PropertyInfo, SourceProperty> _sourceProperties
            = new Dictionary<PropertyInfo, SourceProperty>();

        protected readonly Dictionary<PropertyInfo, TargetProperty> _targetProperties
            = new Dictionary<PropertyInfo, TargetProperty>();

        protected readonly TypeMapping _typeMapping;
        protected readonly GlobalConfiguration _globalConfiguration;

        public TypeMappingConfigurator(TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration)
        {
            _typeMapping = typeMapping;
            _globalConfiguration = globalConfiguration;

            if (!typeMapping.IgnoreConventions)
                this.MapByConvention(typeMapping);
        }

        public TypeMappingConfigurator(TypePair typePair, GlobalConfiguration globalConfiguration)
            : this(new TypeMapping(globalConfiguration, typePair), globalConfiguration) { }

        public TypeMappingConfigurator(Type sourceType, Type targetType, GlobalConfiguration globalConfiguration)
            : this(new TypeMapping(globalConfiguration, new TypePair(sourceType, targetType)), globalConfiguration) { }

        protected SourceProperty GetOrAddSourceProperty(PropertyInfo propertyInfo)
        {
            SourceProperty sourceProperty;
            if (!_sourceProperties.TryGetValue(propertyInfo, out sourceProperty))
            {
                sourceProperty = new SourceProperty(propertyInfo);
                _sourceProperties.Add(propertyInfo, sourceProperty);
            }

            return sourceProperty;
        }

        protected TargetProperty GetOrAddTargetProperty(PropertyInfo propertyInfo)
        {
            TargetProperty targetProperty;
            if (!_targetProperties.TryGetValue(propertyInfo, out targetProperty))
            {
                targetProperty = new TargetProperty(propertyInfo);
                _targetProperties.Add(propertyInfo, targetProperty);
            }

            return targetProperty;
        }

        protected internal void MapByConvention(TypeMapping typeMapping)
        {
            var source = typeMapping.TypePair.SourceType;
            var target = typeMapping.TypePair.TargetType;

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;

            var sourceProperties = source.GetProperties(bindingAttributes)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0); //no indexed properties

            var targetProperties = target.GetProperties(bindingAttributes)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0); //no indexed properties

            foreach (var sourceProperty in sourceProperties)
            {
                foreach (var targetProperty in targetProperties)
                {
                    if (targetProperty.SetMethod != null)
                    {
                        if (_globalConfiguration.MappingConvention.IsMatch(sourceProperty, targetProperty))
                        {
                            var sourcePropertyConfig = this.GetOrAddSourceProperty(sourceProperty);
                            var targetPropertyConfig = this.GetOrAddTargetProperty(targetProperty);

                            var propertyMapping = new PropertyMapping(typeMapping, sourcePropertyConfig, targetPropertyConfig)
                            {
                                MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION
                            };

                            propertyMapping.Mapper = _globalConfiguration.Mappers.FirstOrDefault(
                                mapper => mapper.CanHandle(propertyMapping));

                            if (propertyMapping.Mapper == null)
                                throw new Exception($"No object mapper can handle {propertyMapping}");

                            if (!typeMapping.PropertyMappings.ContainsKey(targetProperty))
                                typeMapping.PropertyMappings.Add(targetProperty, propertyMapping);
                            else
                                typeMapping.PropertyMappings[targetProperty] = propertyMapping;

                            break; //sourceProperty is now mapped, jump directly to the next sourceProperty
                        }
                    }
                }
            }
        }
    }

    public class TypeMappingConfigurator<TSource, TTarget> : TypeMappingConfigurator
    {
        public TypeMappingConfigurator(GlobalConfiguration globalConfiguration) :
            base(typeof(TSource), typeof(TTarget), globalConfiguration)
        { }

        public TypeMappingConfigurator(TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration) : base(typeMapping, globalConfiguration)
        { }

        public TypeMappingConfigurator<TSource, TTarget> TargetConstructor(
            Expression<Func<TSource, TTarget>> constructor)
        {
            _typeMapping.CustomTargetConstructor = constructor;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> EnableMappingConventions()
        {
            _typeMapping.IgnoreConventions = false;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> IgnoreSourceProperty<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            params Expression<Func<TSource, TSourceProperty>>[] sourcePropertySelectors)
        {
            var selectors = new[] { sourcePropertySelector }.Concat(sourcePropertySelectors);
            var properties = selectors.Select(prop => prop.ExtractProperty());

            foreach (var property in properties)
                this.GetOrAddSourceProperty(property).Ignore = true;

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter = null)
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractProperty();
            var targetPropertyInfo = targetPropertySelector.ExtractProperty();

            return this.MapProperty(sourcePropertyInfo, targetPropertyInfo, converter);
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty(string sourcePropertyName,
            string targetPropertyName, LambdaExpression converter = null)
        {
            var sourcePropertyInfo = _typeMapping.TypePair
                .SourceType.GetProperty(sourcePropertyName);

            var targetPropertyInfo = _typeMapping.TypePair
                .TargetType.GetProperty(targetPropertyName);

            return this.MapProperty(sourcePropertyInfo, targetPropertyInfo, converter);
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty(PropertyInfo sourceProperty,
            PropertyInfo targetProperty, LambdaExpression converter = null)
        {
            if (sourceProperty.ReflectedType != _typeMapping.TypePair.SourceType)
                throw new ArgumentException($"'{sourceProperty}' does not belong to type '{_typeMapping.TypePair.SourceType}'");

            if (targetProperty.ReflectedType != _typeMapping.TypePair.TargetType)
                throw new ArgumentException($"'{targetProperty}' does not belong to type '{_typeMapping.TypePair.TargetType}'");

            var sourcePropConfig = this.GetOrAddSourceProperty(sourceProperty);
            var targetPropConfig = this.GetOrAddTargetProperty(targetProperty);

            var propertyMapping = new PropertyMapping(_typeMapping, sourcePropConfig, targetPropConfig)
            {
                MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION,
                CustomConverter = converter
            };

            propertyMapping.Mapper = _globalConfiguration.Mappers.FirstOrDefault(
                mapper => mapper.CanHandle(propertyMapping));

            if (_typeMapping.PropertyMappings.ContainsKey(targetProperty))
                _typeMapping.PropertyMappings[targetProperty] = propertyMapping;
            else
                _typeMapping.PropertyMappings.Add(targetProperty, propertyMapping);

            return this;
        }


        //public TypeMappingConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
        //   Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
        //   Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
        //   ICollectionMappingStrategy collectionStrategy,
        //   Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
        //   where TTargetProperty : IEnumerable
        //{
        //    var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
        //    var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

        //    var propertyMapping = base.Map( sourcePropertyInfo, targetPropertyInfo, converter );
        //    propertyMapping.TargetProperty.CollectionStrategy = collectionStrategy;

        //    return this;
        //}
    }
}

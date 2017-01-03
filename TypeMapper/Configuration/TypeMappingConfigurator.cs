using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Configuration
{
    public class TypeMappingConfigurator
    {
        protected readonly TypeMapping _typeMapping;
        protected readonly GlobalConfiguration _globalConfiguration;

        public TypeMappingConfigurator( TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration )
        {
            _typeMapping = typeMapping;
            _globalConfiguration = globalConfiguration;
        }

        public TypeMappingConfigurator( TypePair typePair,
            GlobalConfiguration globalConfiguration )
        {
            _typeMapping = new TypeMapping( _globalConfiguration, typePair );
            _globalConfiguration = globalConfiguration;
        }

        public TypeMappingConfigurator( Type source, Type target,
            GlobalConfiguration globalConfiguration )
        {
            var typePair = new TypePair( source, target );
            _typeMapping = new TypeMapping( _globalConfiguration, typePair );

            _globalConfiguration = globalConfiguration;
        }
    }

    public class TypeMappingConfigurator<TSource, TTarget> : TypeMappingConfigurator
    {
        public TypeMappingConfigurator( GlobalConfiguration globalConfiguration ) :
            base( typeof( TSource ), typeof( TTarget ), globalConfiguration )
        { }

        public TypeMappingConfigurator( TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration ) : base( typeMapping, globalConfiguration )
        { }

        public TypeMappingConfigurator<TSource, TTarget> TargetConstructor(
            Expression<Func<TSource, TTarget>> constructor )
        {
            _typeMapping.CustomTargetConstructor = constructor;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> EnableMappingConventions()
        {
            _typeMapping.IgnoreConventions = true;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> IgnoreSourceProperty<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            params Expression<Func<TSource, TSourceProperty>>[] sourcePropertySelectors )
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
            _typeMapping.IgnoredSourceProperties.Add( sourcePropertyInfo );

            if( sourcePropertySelectors != null )
            {
                var properties = sourcePropertySelectors
                    .Select( prop => prop.ExtractPropertyInfo() );

                foreach( var property in properties )
                    _typeMapping.IgnoredSourceProperties.Add( property );
            }

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
        {
            var sourcePropertyInfo = targetPropertySelector.ExtractPropertyInfo();
            var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

            PropertyMapping propertyMapping;
            if( !_typeMapping.PropertyMappings.TryGetValue( targetPropertyInfo, out propertyMapping ) )
            {
                propertyMapping = new PropertyMapping( _typeMapping, sourcePropertyInfo, targetPropertyInfo )
                {
                    MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION,
                    CustomConverter = converter
                };

                propertyMapping.Mapper = _globalConfiguration.Mappers.FirstOrDefault(
                    mapper => mapper.CanHandle( propertyMapping ) );

                _typeMapping.PropertyMappings.Add( targetPropertyInfo, propertyMapping );
            }

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

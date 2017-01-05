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
        protected readonly TypeMapping _typeMapping;
        protected readonly GlobalConfiguration _globalConfiguration;

        protected readonly PropertyInfo[] _sourceProperties;
        protected readonly PropertyInfo[] _targetProperties;

        public TypeMappingConfigurator( TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration )
        {
            _typeMapping = typeMapping;
            _globalConfiguration = globalConfiguration;

            _sourceProperties = typeMapping.TypePair.SourceType.GetProperties();
            _targetProperties = typeMapping.TypePair.TargetType.GetProperties();
        }

        public TypeMappingConfigurator( TypePair typePair,
            GlobalConfiguration globalConfiguration )
        {
            _typeMapping = new TypeMapping( _globalConfiguration, typePair );
            _globalConfiguration = globalConfiguration;

            _sourceProperties = typePair.SourceType.GetProperties();
            _targetProperties = typePair.TargetType.GetProperties();
        }

        public TypeMappingConfigurator( Type sourceType, Type targetType,
            GlobalConfiguration globalConfiguration )
        {
            _sourceProperties = sourceType.GetProperties();
            _targetProperties = targetType.GetProperties();

            var typePair = new TypePair( sourceType, targetType );
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
            _typeMapping.IgnoreConventions = false;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> IgnoreSourceProperty<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            params Expression<Func<TSource, TSourceProperty>>[] sourcePropertySelectors )
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractProperty();
            _typeMapping.IgnoredSourceProperties.Add( sourcePropertyInfo );

            if( sourcePropertySelectors != null )
            {
                var properties = sourcePropertySelectors
                    .Select( prop => prop.ExtractProperty() );

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
            var sourcePropertyInfo = sourcePropertySelector.ExtractProperty();
            var targetPropertyInfo = targetPropertySelector.ExtractProperty();

            return this.MapProperty( sourcePropertyInfo, targetPropertyInfo, converter );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty( string sourcePropertyName,
            string targetPropertyName, LambdaExpression converter = null )
        {
            var sourcePropertyInfo = _typeMapping.TypePair
                .SourceType.GetProperty( sourcePropertyName );

            var targetPropertyInfo = _typeMapping.TypePair
                .TargetType.GetProperty( targetPropertyName );

            return this.MapProperty( sourcePropertyInfo, targetPropertyInfo, converter );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty( PropertyInfo sourceProperty,
            PropertyInfo targetProperty, LambdaExpression converter = null )
        {
            if( sourceProperty.ReflectedType != _typeMapping.TypePair.SourceType )
                throw new ArgumentException( $"'{sourceProperty}' does not belong to type '{_typeMapping.TypePair.SourceType}'" );

            if( targetProperty.ReflectedType != _typeMapping.TypePair.TargetType )
                throw new ArgumentException( $"'{targetProperty}' does not belong to type '{_typeMapping.TypePair.TargetType}'" );

            var propertyMapping = new PropertyMapping( _typeMapping,
                sourceProperty, targetProperty )
            {
                MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION,
                CustomConverter = converter
            };

            propertyMapping.Mapper = _globalConfiguration.Mappers.FirstOrDefault(
                mapper => mapper.CanHandle( propertyMapping ) );

            if( _typeMapping.PropertyMappings.ContainsKey( targetProperty ) )
                _typeMapping.PropertyMappings[ targetProperty ] = propertyMapping;
            else
                _typeMapping.PropertyMappings.Add( targetProperty, propertyMapping );

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

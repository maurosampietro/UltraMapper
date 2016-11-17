using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class TypeConfiguration
    {
        private Dictionary<TypePair, PropertyConfiguration> _typeMappings =
            new Dictionary<TypePair, PropertyConfiguration>();

        public PropertyConfiguration<TSource, TDestination> Map<TSource, TDestination>()
        {
            var map = this.Map( typeof( TSource ), typeof( TDestination ) );
            return new PropertyConfiguration<TSource, TDestination>( map );
        }

        public PropertyConfiguration Map( Type source, Type destination )
        {
            var typePair = new TypePair( source, destination );

            PropertyConfiguration typeMapping;
            if( _typeMappings.TryGetValue( typePair, out typeMapping ) )
                return typeMapping;

            var propertymappings = new PropertyConfiguration( source, destination );
            _typeMappings.Add( typePair, propertymappings );

            return propertymappings;
        }

        public PropertyConfiguration this[ TypePair key ]
        {
            get
            {
                PropertyConfiguration typeMapping = null;
                _typeMappings.TryGetValue( key, out typeMapping );

                return typeMapping;
            }
        }

        public PropertyConfiguration this[ Type sourceType, Type destinationType ]
        {
            get
            {
                var typePair = new TypePair( sourceType, destinationType );
                return this[ typePair ];
            }
        }
    }

    public class PropertyConfiguration
    {
        //A source property can be mapped to multiple destination properties
        protected Dictionary<PropertyInfoPair, PropertyMapping> _propertyMappings;

        /// <summary>
        /// This constructor is only used by derived classes to allow
        /// casts from PropertyConfiguration to the derived class itself
        /// </summary>
        /// <param name="configuration">An already existing mapping configuration</param>
        protected PropertyConfiguration( PropertyConfiguration configuration )
        {
            _propertyMappings = configuration._propertyMappings;
        }

        public PropertyConfiguration( Type source, Type destination )
            : this( source, destination, new SameNameAndTypeConvention() ) { }

        public PropertyConfiguration( Type source, Type destination, IMappingConvention mappingConvention )
        {
            _propertyMappings = new Dictionary<PropertyInfoPair, PropertyMapping>();

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;

            var sourceProperties = source.GetProperties( bindingAttributes )
                .Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

            var destinationProperties = destination.GetProperties( bindingAttributes )
                .Where( p => p.CanWrite && p.GetIndexParameters().Length == 0 ); //no indexed properties

            foreach( var sourceProperty in sourceProperties )
            {
                foreach( var destinationProperty in destinationProperties )
                {
                    if( destinationProperty.SetMethod != null )
                    {
                        if( mappingConvention.IsMatch( sourceProperty, destinationProperty ) )
                        {
                            this.Map( sourceProperty, destinationProperty );
                            break; //sourceProperty is now mapped, jump directly to the next sourceProperty
                        }
                    }
                }
            }
        }

        protected PropertyMapping Map( PropertyInfo sourcePropertyInfo, PropertyInfo destinationPropertyInfo )
        {
            var typePairKey = new PropertyInfoPair( sourcePropertyInfo, destinationPropertyInfo );

            PropertyMapping propertyMapping;
            if( !_propertyMappings.TryGetValue( typePairKey, out propertyMapping ) )
            {
                var sourceProperty = new SourceProperty()
                {
                    PropertyInfo = sourcePropertyInfo,
                    IsBuiltInType = sourcePropertyInfo.PropertyType.IsBuiltInType( true ),
                    IsEnumerable = sourcePropertyInfo.PropertyType.IsEnumerable(),
                    ValueGetter = FastInvoke.BuildUntypedCastGetter( sourcePropertyInfo )
                };

                propertyMapping = new PropertyMapping( sourceProperty );
                _propertyMappings.Add( typePairKey, propertyMapping );
            }

            propertyMapping.DestinationProperty = new DestinationProperty()
            {
                PropertyInfo = destinationPropertyInfo,
                NullableUnderlyingType = Nullable.GetUnderlyingType( destinationPropertyInfo.PropertyType ),
                ValueSetter = FastInvoke.BuildUntypedCastSetter( destinationPropertyInfo )
            };

            return propertyMapping;
        }

        internal PropertyMapping this[ PropertyInfo sourceProperty, PropertyInfo destinationProperty ]
        {
            get
            {
                var typePairKey = new PropertyInfoPair( sourceProperty, destinationProperty );
                return _propertyMappings[ typePairKey ];
            }
        }

        internal IEnumerable<PropertyMapping> GetPropertyMappings()
        {
            //The internal dictionary is too complex to be used externally.
            //Return just the mappings themselves, no matter the internal type used.
            return _propertyMappings.Values;
        }
    }

    public class PropertyConfiguration<TSource, TDestination> : PropertyConfiguration
    {
        /// <summary>
        /// This constructor is only used internally to allow
        /// casts from PropertyConfiguration to ProertyConfiguration<>
        /// </summary>
        /// <param name="map">An already existing mapping configuration</param>
        internal PropertyConfiguration( PropertyConfiguration map )
            : base( map ) { }

        public PropertyConfiguration() : this( new SameNameAndTypeConvention() ) { }

        public PropertyConfiguration( IMappingConvention mappingConvention )
            : base( typeof( TSource ), typeof( TDestination ), mappingConvention ) { }

        public PropertyConfiguration<TSource, TDestination> MapProperty<TSourceProperty, TDestinationProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TDestination, TDestinationProperty>> destinationPropertySelector,
            Expression<Func<TSourceProperty, TDestinationProperty>> converter )
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
            var destinationPropertyInfo = destinationPropertySelector.ExtractPropertyInfo();

            var propertyMapping = this.Map( sourcePropertyInfo, destinationPropertyInfo );

            propertyMapping.ValueConverter = (converter == null) ? null :
                converter.EncapsulateInGenericFunc<TSourceProperty>().Compile();

            return this;
        }

        public PropertyConfiguration<TSource, TDestination> MapProperty<TSourceProperty, TDestinationProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TDestination, TDestinationProperty>> destinationPropertySelector )
        {
            return MapProperty( sourcePropertySelector, destinationPropertySelector, null );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;
using TypeMapper.MappingConventions;

namespace TypeMapper.Configuration
{
    public class PropertyConfiguration
    {
        //A source property can be mapped to multiple target properties
        private Dictionary<PropertyInfoPair, PropertyMapping> _propertyMappings;

        /// <summary>
        /// This constructor is only used by derived classes to allow
        /// casts from PropertyConfiguration to the derived class itself
        /// </summary>
        /// <param name="configuration">An already existing mapping configuration</param>
        protected PropertyConfiguration( PropertyConfiguration configuration )
        {
            _propertyMappings = configuration._propertyMappings;
        }

        public PropertyConfiguration( Type source, Type target )
            : this( source, target, new DefaultMappingConvention() ) { }

        public PropertyConfiguration( Type source, Type target, IMappingConvention mappingConvention )
        {
            _propertyMappings = new Dictionary<PropertyInfoPair, PropertyMapping>();

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;

            var sourceProperties = source.GetProperties( bindingAttributes )
                .Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

            var targetProperties = target.GetProperties( bindingAttributes )
                .Where( p => p.CanWrite && p.GetIndexParameters().Length == 0 ); //no indexed properties

            foreach( var sourceProperty in sourceProperties )
            {
                foreach( var targetProperty in targetProperties )
                {
                    if( targetProperty.SetMethod != null )
                    {
                        if( mappingConvention.IsMatch( sourceProperty, targetProperty ) )
                        {
                            this.Map( sourceProperty, targetProperty );
                            break; //sourceProperty is now mapped, jump directly to the next sourceProperty
                        }
                    }
                }
            }
        }

        protected PropertyMapping Map( PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo )
        {
            var typePairKey = new PropertyInfoPair( sourcePropertyInfo, targetPropertyInfo );

            PropertyMapping propertyMapping;
            if( !_propertyMappings.TryGetValue( typePairKey, out propertyMapping ) )
            {
                var sourceProperty = new SourceProperty( sourcePropertyInfo )
                {
                    IsBuiltInType = sourcePropertyInfo.PropertyType.IsBuiltInType( true ),
                    IsEnumerable = sourcePropertyInfo.PropertyType.IsEnumerable(),
                    ValueGetter = FastInvoke.BuildUntypedCastGetter( sourcePropertyInfo )
                };

                propertyMapping = new PropertyMapping( sourceProperty );
                _propertyMappings.Add( typePairKey, propertyMapping );
            }

            propertyMapping.TargetProperty = new TargetProperty( targetPropertyInfo )
            {
                NullableUnderlyingType = Nullable.GetUnderlyingType( targetPropertyInfo.PropertyType ),
                ValueSetter = FastInvoke.BuildUntypedCastSetter( targetPropertyInfo )
            };

            return propertyMapping;
        }

        internal PropertyMapping this[ PropertyInfo sourceProperty, PropertyInfo targetProperty ]
        {
            get
            {
                var typePairKey = new PropertyInfoPair( sourceProperty, targetProperty );
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

    public class PropertyConfiguration<TSource, TTarget> : PropertyConfiguration
    {
        /// <summary>
        /// This constructor is only used internally to allow
        /// casts from PropertyConfiguration to ProertyConfiguration<>
        /// </summary>
        /// <param name="map">An already existing mapping configuration</param>
        internal PropertyConfiguration( PropertyConfiguration map )
            : base( map ) { }

        public PropertyConfiguration( IMappingConvention mappingConvention )
            : base( typeof( TSource ), typeof( TTarget ), mappingConvention ) { }

        public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter )
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
            var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

            var propertyMapping = base.Map( sourcePropertyInfo, targetPropertyInfo );

            propertyMapping.ValueConverter = (converter == null) ? null :
                converter.EncapsulateInGenericFunc<TSourceProperty>().Compile();

            return this;
        }

        public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector )
        {
            return MapProperty( sourcePropertySelector, targetPropertySelector, null );
        }

        //public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
        //   Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
        //   Expression<Func<TTarget, TTargetProperty>> targetPropertySelector ) 
        //where TSourceProperty:IEnumerable 
        //where TTargetProperty : IEnumerable
        //{
        //    return null; // MapProperty( sourcePropertySelector, targetPropertySelector, null );
        //}
    }
}

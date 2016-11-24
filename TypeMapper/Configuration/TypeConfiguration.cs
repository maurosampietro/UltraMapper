using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.MappingConventions;

namespace TypeMapper.Configuration
{
    public class TypeConfiguration<T> : TypeConfiguration where T : IMappingConvention, new()
    {
        public TypeConfiguration() { }

        public TypeConfiguration( Action<T> config )
              : base( new T() ) { config?.Invoke( (T)_mappingConvention ); }
    }

    public class TypeConfiguration
    {
        private Dictionary<TypePair, PropertyConfiguration> _typeMappings =
            new Dictionary<TypePair, PropertyConfiguration>();

        protected IMappingConvention _mappingConvention;
        public IMappingConvention MappingConvention { get { return _mappingConvention; } }

        public TypeConfiguration()
        {
            _mappingConvention = new DefaultMappingConvention();
        }

        public TypeConfiguration( Action<DefaultMappingConvention> config )
            : this() { config?.Invoke( (DefaultMappingConvention)_mappingConvention ); }

        public TypeConfiguration( IMappingConvention mappingConvention )
        {
            _mappingConvention = mappingConvention;
        }

        public PropertyConfiguration<TSource, TTarget> MapTypes<TSource, TTarget>( TSource source, TTarget target )
        {
            return MapTypes<TSource, TTarget>();
        }

        public PropertyConfiguration<TSource, TTarget> MapTypes<TSource, TTarget>()
        {
            var map = this.MapTypes( typeof( TSource ), typeof( TTarget ) );
            return new PropertyConfiguration<TSource, TTarget>( map );
        }

        public PropertyConfiguration MapTypes( Type source, Type target )
        {
            var typePair = new TypePair( source, target );

            PropertyConfiguration typeMapping;
            if( _typeMappings.TryGetValue( typePair, out typeMapping ) )
                return typeMapping;

            var propertymappings = new PropertyConfiguration( source, target, _mappingConvention );
            _typeMappings.Add( typePair, propertymappings );

            return propertymappings;
        }

        /// <summary>
        /// Gets the property mapping associated with the source/target type pair.
        /// If the mapping for the pair does not exist, it is created.
        /// </summary>
        /// <param name="key">The source/target type pair</param>
        /// <returns>The mapping associated with the type pair</returns>
        public PropertyConfiguration this[ Type sourceType, Type targetType ]
        {
            get
            {
                var typePair = new TypePair( sourceType, targetType );
                return this[ typePair ];
            }
        }

        /// <summary>
        /// Gets the property mapping associated with the source/target type pair.
        /// If the mapping for the pair does not exist, it is created.
        /// </summary>
        /// <param name="key">The source/target type pair</param>
        /// <returns>The mapping associated with the type pair</returns>
        internal PropertyConfiguration this[ TypePair key ]
        {
            get
            {
                PropertyConfiguration typeMapping = null;
                if( !_typeMappings.TryGetValue( key, out typeMapping ) )
                    typeMapping = this.MapTypes( key.SourceType, key.DestinationType );

                return typeMapping;
            }
        }
    }
}

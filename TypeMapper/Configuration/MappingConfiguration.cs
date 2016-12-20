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
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace TypeMapper.Configuration
{
    public class MappingConfiguration<T> : MappingConfiguration where T : IMappingConvention, new()
    {
        public MappingConfiguration() { }

        public MappingConfiguration( Action<T> config )
              : base( new T() ) { config?.Invoke( (T)this.MappingConvention ); }
    }

    public class MappingConfiguration
    {
        private Dictionary<TypePair, TypeMappingConfiguration> _typeMappings =
            new Dictionary<TypePair, TypeMappingConfiguration>();

        public IMappingConvention MappingConvention { get; protected set; }
        public ObjectMapperConfiguration ObjectMappers { get; set; }

        public MappingConfiguration()
        {
            this.MappingConvention = new DefaultMappingConvention();
            this.ObjectMappers = new ObjectMapperConfiguration()
                .Add<CustomConverterMapper>()
                .Add<BuiltInTypeMapper>()
                .Add<NullableMapper>()
                .Add<ConvertMapper>()
                .Add<ReferenceMapper>()
                .Add<DictionaryMapper>() //since dictionaries are collections, to be correctly handled must be evaluated by a suitable mapper before a CollectionMapper
                .Add<CollectionMapper>();
        }

        public MappingConfiguration( Action<DefaultMappingConvention> config )
            : this()
        {
            config?.Invoke( (DefaultMappingConvention)this.MappingConvention );
        }

        public MappingConfiguration( IMappingConvention mappingConvention )
        {
            this.MappingConvention = mappingConvention;
        }

        public TypeMappingConfiguration<TSource, TTarget> MapTypes<TSource, TTarget>(
            TSource source, TTarget target, bool ignoreConventionMappings = false )
        {
            return MapTypes<TSource, TTarget>();
        }

        public TypeMappingConfiguration<TSource, TTarget> MapTypes<TSource, TTarget>( bool ignoreConventionMappings = false )
        {
            var map = this.MapTypes( typeof( TSource ), typeof( TTarget ), ignoreConventionMappings );
            return new TypeMappingConfiguration<TSource, TTarget>( map, this.ObjectMappers, ignoreConventionMappings );
        }

        public TypeMappingConfiguration MapTypes( Type source, Type target, bool ignoreConventionMappings = false )
        {
            var typePair = new TypePair( source, target );

            TypeMappingConfiguration typeMapping;
            if( _typeMappings.TryGetValue( typePair, out typeMapping ) )
                return typeMapping;

            var propertymappings = new TypeMappingConfiguration( source, target,
                this.MappingConvention, this.ObjectMappers, ignoreConventionMappings );

            _typeMappings.Add( typePair, propertymappings );

            return propertymappings;
        }

        /// <summary>
        /// Gets the property mapping associated with the source/target type pair.
        /// If the mapping for the pair does not exist, it is created.
        /// </summary>
        /// <param name="key">The source/target type pair</param>
        /// <returns>The mapping associated with the type pair</returns>
        public TypeMappingConfiguration this[ Type sourceType, Type targetType ]
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
        internal TypeMappingConfiguration this[ TypePair key ]
        {
            get
            {
                TypeMappingConfiguration typeMapping = null;
                if( !_typeMappings.TryGetValue( key, out typeMapping ) )
                    typeMapping = this.MapTypes( key.SourceType, key.TargetType );

                return typeMapping;
            }
        }
    }
}

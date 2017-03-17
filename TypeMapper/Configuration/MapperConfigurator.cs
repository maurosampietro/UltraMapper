using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TypeMapper.Configuration;
using TypeMapper.ExtensionMethods;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.Mappers.TypeMappers;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class MapperConfiguration<T> : MapperConfiguration where T : IMappingConvention, new()
    {
        public MapperConfiguration() { }

        public MapperConfiguration( Action<T> config )
              : base( new T() ) { config?.Invoke( (T)GlobalConfiguration.MappingConvention ); }
    }

    public class MapperConfiguration
    {
        protected readonly Dictionary<TypePair, TypeMapping> _typeMappings =
            new Dictionary<TypePair, TypeMapping>();

        public readonly GlobalConfiguration GlobalConfiguration;

        public MapperConfiguration( IMappingConvention mappingConvention )
            : this()
        {
            GlobalConfiguration.MappingConvention = mappingConvention;
        }

        public MapperConfiguration( Action<DefaultMappingConvention> config )
             : this()
        {
            config?.Invoke( (DefaultMappingConvention)GlobalConfiguration.MappingConvention );
        }

        public MapperConfiguration()
        {
            GlobalConfiguration = new GlobalConfiguration( this )
            {
                MappingConvention = new DefaultMappingConvention(),
            };
        }

        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( Action<TypeMapping> typeConfig )
        {
            var typePair = new TypePair( typeof( TSource ), typeof( TTarget ) );

            var typeMapping = _typeMappings.GetOrAdd( typePair,
                () => new TypeMapping( GlobalConfiguration, typePair ) );

            typeConfig?.Invoke( typeMapping );
            return new TypeMappingConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            Expression<Func<TTarget>> targetConstructor = null,
            Expression<Func<TSource, TTarget>> converter = null )
        {
            var typePair = new TypePair( typeof( TSource ), typeof( TTarget ) );

            var typeMapping = _typeMappings.GetOrAdd( typePair,
                () => new TypeMapping( GlobalConfiguration, typePair ) );

            typeMapping.CustomTargetConstructor = targetConstructor;
            typeMapping.CustomConverter = converter;

            return new TypeMappingConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            TSource source, TTarget target )
        {
            var typePair = new TypePair( source.GetType(), target.GetType() );

            var typeMapping = _typeMappings.GetOrAdd( typePair,
                () => new TypeMapping( GlobalConfiguration, typePair ) );

            return new TypeMappingConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
        }

        public TypeMapping this[ Type source, Type target ]
        {
            get
            {
                var typePair = new TypePair( source, target );

                TypeMapping typeMapping;
                if( _typeMappings.TryGetValue( typePair, out typeMapping ) )
                    return typeMapping;
   
                typeMapping = new TypeMapping( GlobalConfiguration, typePair );
                new TypeMappingConfigurator( typeMapping, GlobalConfiguration );

                _typeMappings.Add( typePair, typeMapping );
                return typeMapping;
            }
        }
    }
}

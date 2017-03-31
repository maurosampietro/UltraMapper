using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Configuration;
using UltraMapper.ExtensionMethods;
using UltraMapper.Internals;
using UltraMapper.Mappers;
using UltraMapper.MappingConventions;

namespace UltraMapper
{
    public class TypeConfigurator<T> : TypeConfigurator where T : IMappingConvention, new()
    {
        public TypeConfigurator() { }

        public TypeConfigurator( Action<T> config )
              : base( new T() ) { config?.Invoke( (T)GlobalConfiguration.MappingConvention ); }
    }

    public class TypeConfigurator
    {
        protected readonly Dictionary<TypePair, TypeMapping> _typeMappings =
            new Dictionary<TypePair, TypeMapping>();

        public readonly GlobalConfiguration GlobalConfiguration;

        public TypeConfigurator( IMappingConvention mappingConvention )
            : this()
        {
            GlobalConfiguration.MappingConvention = mappingConvention;
        }

        public TypeConfigurator( Action<DefaultMappingConvention> config )
             : this()
        {
            config?.Invoke( (DefaultMappingConvention)GlobalConfiguration.MappingConvention );
        }

        public TypeConfigurator()
        {
            GlobalConfiguration = new GlobalConfiguration( this )
            {
                MappingConvention = new DefaultMappingConvention(),
            };
        }

        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( Action<TypeMapping> typeConfig )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeConfig?.Invoke( typeMapping );

            return new TypeMappingConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The conversion mechanism to be used to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            Expression<Func<TSource, TTarget>> converter )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMapping.CustomConverter = converter;

            return new TypeMappingConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The expression providing an instance of <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            Expression<Func<TTarget>> targetConstructor = null )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMapping.CustomTargetConstructor = targetConstructor;

            return new TypeMappingConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="source">Source instance</param>
        /// <param name="target">Target instance</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public TypeMappingConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( TSource source, TTarget target )
        {
            var typeMapping = this.GetTypeMapping( source.GetType(), target.GetType() );
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

                //configure by convention
                new MemberConfigurator( typeMapping, GlobalConfiguration );

                _typeMappings.Add( typePair, typeMapping );
                return typeMapping;
            }
        }

        private TypeMapping GetTypeMapping( Type source, Type target )
        {
            var typePair = new TypePair( source, target );

            return _typeMappings.GetOrAdd( typePair,
                () => new TypeMapping( GlobalConfiguration, typePair ) );
        }
    }
}

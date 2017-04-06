using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.Mappers;
using UltraMapper.MappingConventions;
using UltraMapper.ExtensionMethods;

namespace UltraMapper
{
    public enum ReferenceMappingStrategies { CREATE_NEW_INSTANCE, USE_TARGET_INSTANCE_IF_NOT_NULL }

    public interface IMappingOptions
    {
        CollectionMappingStrategies CollectionMappingStrategy { get; set; }
        ReferenceMappingStrategies ReferenceMappingStrategy { get; set; }
    }

    public interface IMemberOptions : IMappingOptions
    {
        bool Ignore { get; set; }
    }

    public interface ITypeOptions : IMappingOptions
    {
        bool IgnoreMemberMappingResolvedByConvention { get; set; }
    }

    public class Configuration
    {
        private readonly Dictionary<TypePair, TypeMapping> _typeMappings =
            new Dictionary<TypePair, TypeMapping>();

        /// <summary>
        /// If set to True only explicitly user-defined member-mappings are 
        /// taken into account in the mapping process.
        /// 
        /// If set to False members-mappings that have been resolved by convention 
        /// are taken into account in the mapping process.
        /// </summary>
        public bool IgnoreMemberMappingResolvedByConvention { get; set; }

        public CollectionMappingStrategies CollectionMappingStrategy { get; set; }
        public ReferenceMappingStrategies ReferenceMappingStrategy { get; set; }

        public IMappingConvention MappingConvention { get; set; }
        public List<IMapperExpressionBuilder> Mappers { get; private set; }

        public Configuration( Action<Configuration> config = null )
        {
            this.MappingConvention = new DefaultMappingConvention();

            this.ReferenceMappingStrategy = ReferenceMappingStrategies.CREATE_NEW_INSTANCE;
            this.CollectionMappingStrategy = CollectionMappingStrategies.RESET;

            this.Mappers = new List<IMapperExpressionBuilder>()
            {
                //Order is important: the first MapperExpressionBuilder that can handle a mapping is used.
                //Make sure to use a collection which preserve insertion order!
                new BuiltInTypeMapper( this ),
                new NullableMapper( this ),
                new ConvertMapper( this ),
                new StructMapper( this ),
                new DictionaryMapper( this ),
                new StackMapper( this ),
                new QueueMapper( this ),
                new LinkedListMapper( this ),
                new CollectionMapper( this ),
                new ReferenceMapper( this ),
            };

            config?.Invoke( this );
        }

        public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( Action<ITypeOptions> typeMappingConfig = null )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The conversion mechanism to be used to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            Expression<Func<TSource, TTarget>> converter, Action<ITypeOptions> typeMappingConfig = null )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMapping.CustomConverter = converter;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The expression providing an instance of <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
            Expression<Func<TTarget>> targetConstructor, Action<ITypeOptions> typeMappingConfig = null )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMapping.CustomTargetConstructor = targetConstructor;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
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
        public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( TSource source, TTarget target,
            Action<ITypeOptions> typeMappingConfig = null )
        {
            var typeMapping = this.GetTypeMapping( source.GetType(), target.GetType() );
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        private TypeMapping GetTypeMapping( Type source, Type target )
        {
            var typePair = new TypePair( source, target );

            return _typeMappings.GetOrAdd( typePair,
                () => new TypeMapping( this, typePair ) );
        }

        public TypeMapping this[ Type source, Type target ]
        {
            get
            {
                var typePair = new TypePair( source, target );

                TypeMapping typeMapping;
                if( _typeMappings.TryGetValue( typePair, out typeMapping ) )
                    return typeMapping;

                typeMapping = new TypeMapping( this, typePair );

                //configure by convention
                new MemberConfigurator( typeMapping );

                _typeMappings.Add( typePair, typeMapping );
                return typeMapping;
            }
        }
    }
}
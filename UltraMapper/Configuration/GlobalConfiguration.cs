using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Conventions;
using System.Collections;

namespace UltraMapper
{
    public enum ReferenceMappingStrategies
    {
        /// <summary>
        /// Creates a new instance, but only if the reference has not been mapped and tracked yet.
        /// If the reference has been mapped and tracked, the tracked object is assigned.
        /// This is the default.
        /// </summary>
        CREATE_NEW_INSTANCE,

        /// <summary>
        /// The instance of the target is used in one particular case, following this table:
        /// SOURCE (NULL) -> TARGET = NULL
        /// 
        /// SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
        /// SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
        /// 
        /// SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NULL) = ASSIGN NEW OBJECT 
        /// SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
        /// </summary>
        USE_TARGET_INSTANCE_IF_NOT_NULL
    }

    public enum CollectionMappingStrategies
    {
        /// <summary>
        /// Keeps using the input collection (same reference). 
        /// The collection is cleared and then elements are added. 
        /// </summary>
        RESET,

        /// <summary>
        /// Keep using the input collection (same reference).
        /// The collection is untouched and elements are added.
        /// </summary>
        MERGE,

        /// <summary>
        /// Keeps using the input collection (same reference).
        /// Each source item matching a target item is updated.
        /// Each source item non existing in the target collection is added.
        /// Each target item non existing in the source collection is removed.
        /// A way to compare two items must also be provided.
        /// </summary>
        UPDATE
    }

    public interface IMappingOptions
    {
        CollectionMappingStrategies CollectionMappingStrategy { get; set; }
        ReferenceMappingStrategies ReferenceMappingStrategy { get; set; }

        LambdaExpression CollectionItemEqualityComparer { get; set; }
        LambdaExpression CustomTargetConstructor { get; set; }
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

        public List<IMappingExpressionBuilder> Mappers { get; set; }
        public MappingConventions Conventions { get; set; }

        public Configuration( Action<Configuration> config = null )
        {
            this.Conventions = new MappingConventions( cfg =>
            {
                cfg.GetOrAdd<DefaultConvention>();
            } );

            this.ReferenceMappingStrategy = ReferenceMappingStrategies.CREATE_NEW_INSTANCE;
            this.CollectionMappingStrategy = CollectionMappingStrategies.RESET;

            this.Mappers = new List<IMappingExpressionBuilder>()
            {
                //Order is important: it is used the first MapperExpressionBuilder able to handle a mapping.
                //Make sure to use a collection which preserve insertion order!
                new StringToEnumMapper( this ),
                new EnumMapper( this ),
                new BuiltInTypeMapper( this ),
                new NullableMapper( this ),
                new ConvertMapper( this ),
                new StructMapper( this ),
                new ArrayMapper( this ),
                new DictionaryMapper( this ),
                new ReadOnlyCollectionMapper( this ),
                new StackMapper( this ),
                new QueueMapper( this ),
                new LinkedListMapper( this ),
                new CollectionMapper( this ),
                new ReferenceMapper( this ),
                new ReferenceToStructMapper( this ),
            };

            config?.Invoke( this );
        }

        #region Type-to-Type Mapping

        public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget, TSourceElement, TTargetElement>(
            Expression<Func<TSourceElement, TTargetElement, bool>> elementEqualityComparison )
            where TSource : IEnumerable
            where TTarget : IEnumerable
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMapping.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
            typeMapping.CollectionMappingStrategy = CollectionMappingStrategies.UPDATE;
            typeMapping.CollectionItemEqualityComparer = elementEqualityComparison;

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }


        //public MemberConfigurator<IEnumerable<TSource>, IEnumerable<TTarget>> MapTypes<TSource, TTarget>(
        //    IEnumerable<TSource> source, IEnumerable<TTarget> target,
        //    Expression<Func<TSource, TTarget, bool>> elementEqualityComparison )
        //{
        //    var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
        //    typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
        //    typeMapping.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
        //    typeMapping.CollectionMappingStrategy = CollectionMappingStrategies.UPDATE;
        //    typeMapping.CollectionItemEqualityComparer = elementEqualityComparison;

        //    return new MemberConfigurator<IEnumerable<TSource>, IEnumerable<TTarget>>( typeMapping );
        //}

        public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( Action<ITypeOptions> typeMappingConfig = null )
        {
            var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
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
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
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
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
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
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }
        #endregion

        public bool ContainsMapping( TypeMapping typeMapping )
        {
            return _typeMappings.ContainsKey( typeMapping.TypePair );
        }

        internal bool ContainsMapping( Type source, Type target )
        {
            var typePair = new TypePair( source, target );
            return _typeMappings.ContainsKey( typePair );
        }

        private TypeMapping GetTypeMapping( Type source, Type target )
        {
            var typePair = new TypePair( source, target );

            return _typeMappings.GetOrAdd( typePair,
                () => new TypeMapping( this, typePair ) );
        }

        public TypeMapping this[ Type source, Type target ]
        {
            get { return GetTypeMapping( source, target ); }
        }
    }
}
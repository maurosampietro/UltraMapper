using System;
using System.Globalization;
using UltraMapper.Config;
using UltraMapper.Conventions;
using UltraMapper.Conventions.Resolvers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper
{
    public sealed class Configuration
    {
        public ConfigInheritanceTree TypeMappingTree { get; }
        //public TargetTypeConfiguration TargetResolution { get; }

        internal readonly GeneratedExpressionCache ExpCache;

        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// If set to True only explicitly user-defined member-mappings are 
        /// taken into account in the mapping process.
        /// 
        /// If set to False members-mappings that have been resolved by convention 
        /// are taken into account in the mapping process.
        /// </summary>
        public bool IgnoreMemberMappingResolvedByConvention { get; set; } = false;

        /// <summary>
        /// Enables or disables interface and abstract types runtime resolution mechanism
        /// </summary>
        public bool IsRuntimeInterfaceAbstractResolutionEnabled { get; set; } = true;

        /// <summary>
        /// Enables or disables the reference tracking mechanism
        /// </summary>
        public bool IsReferenceTrackingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the default strategy used when mapping collections.
        /// </summary>
        public CollectionBehaviors CollectionBehavior
        {
            get { return TypeMappingTree.Root.Item.CollectionBehavior; }
            set { TypeMappingTree.Root.Item.CollectionBehavior = value; }
        }

        /// <summary>
        /// Gets or sets the default strategy used when mapping reference objects.
        /// </summary>
        public ReferenceBehaviors ReferenceBehavior
        {
            get { return TypeMappingTree.Root.Item.ReferenceBehavior; }
            set { TypeMappingTree.Root.Item.ReferenceBehavior = value; }
        }

        public OrderedTypeSet<IMappingExpressionBuilder> Mappers { get; private set; }
        public TypeSet<IMappingConvention> Conventions { get; private set; }
        public IConventionResolver ConventionResolver { get; set; }

        public Configuration( Action<Configuration> config = null )
        {
            var rootMapping = new TypeMapping( this, typeof( object ), typeof( object ) )
            {
                //Must not use INHERIT as a value for root-mapping options
                ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE,
                CollectionBehavior = CollectionBehaviors.RESET
            };

            TypeMappingTree = new ConfigInheritanceTree( rootMapping );
            //TargetResolution = new TargetTypeConfiguration();
            ExpCache = new GeneratedExpressionCache();

            //Order is important: the first MapperExpressionBuilder able to handle a mapping is used.
            //Make sure to use a collection which preserve insertion order!
            this.Mappers = new OrderedTypeSet<IMappingExpressionBuilder>()
            {
                //new MemberMapper( this ),
                //new AbstractMappingExpressionBuilder(this),
                new CopyValueMapper(),
                new StringToEnumMapper(),
                new EnumMapper(),
                new BuiltInTypeMapper(),
                new NullableMapper(),
                new StructMapper(),
                new ConvertMapper(),
                new CollectionToArrayMapper(),
                new EnumerableIteratorToArrayMapper(),
                new DictionaryMapper(),
                new ReadOnlyCollectionMapper(),
                //new SortedSetMapper(),
                new StackMapper(),
                new QueueMapper(),
                new LinkedListMapper(),
                new CollectionMapper(),
                new ReferenceMapper(),
                new ReferenceToStructMapper(),
            };

            this.Conventions = new TypeSet<IMappingConvention>( cfg =>
            {
                cfg.GetOrAdd<DefaultConvention>( conv =>
                {
                    conv.SourceMemberProvider.IgnoreProperties = false;
                    conv.SourceMemberProvider.IgnoreFields = true;
                    conv.SourceMemberProvider.IgnoreNonPublicMembers = true;
                    conv.SourceMemberProvider.IgnoreMethods = true;

                    conv.TargetMemberProvider.IgnoreProperties = false;
                    conv.TargetMemberProvider.IgnoreFields = true;
                    conv.TargetMemberProvider.IgnoreNonPublicMembers = true;
                    conv.TargetMemberProvider.IgnoreMethods = true;

                    conv.MatchingRules
                        .GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = true )
                        .GetOrAdd<PrefixMatching>( rule => rule.IgnoreCase = true )
                        .GetOrAdd<SuffixMatching>( rule => rule.IgnoreCase = true );
                } );
            } );

            this.ConventionResolver = new DefaultConventionResolver();

            BuiltInConverters.AddStringToDateTime( this );
            //BuiltInConverters.AddPrimitiveTypeToItself( this );
            //BuiltInConverters.AddExplicitNumericConverters( this );
            //BuiltInConverters.AddImplicitNumericConverters( this );
            //BuiltInConverters.AddPrimitiveTypeToStringConverters( this );
            //BuiltInConverters.AddStringToPrimitiveTypeConverters( this );

            config?.Invoke( this );
        }

        public TypeMapping this[ Type source, Type target ]
        {
            get
            {
                return this.TypeMappingTree.GetOrAdd( source, target, () =>
                {
                    var newTypeMapping = new TypeMapping( this, source, target );
                    ConventionResolver.MapByConvention( newTypeMapping, this.Conventions );

                    return newTypeMapping;
                } );
            }
        }

        public bool TryGetMapping( Type source, Type target, out TypeMapping typeMapping )
        {
            typeMapping = null;

            if( this.TypeMappingTree.TryGetValue( source, target, out var node ) )
            {
                typeMapping = node.Item;
                return true;
            }

            return false;
        }
    }
}
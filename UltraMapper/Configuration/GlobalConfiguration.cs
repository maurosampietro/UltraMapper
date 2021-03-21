using System;
using System.Collections.Generic;
using UltraMapper.Config;
using UltraMapper.Conventions;
using UltraMapper.Conventions.Resolvers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper
{
    public class Configuration
    {
        internal readonly TypeMappingInheritanceTree TypeMappingTree;
        internal readonly GeneratedExpressionCache ExpCache;
        private readonly IConventionResolver _conventionResolver;

        /// <summary>
        /// If set to True only explicitly user-defined member-mappings are 
        /// taken into account in the mapping process.
        /// 
        /// If set to False members-mappings that have been resolved by convention 
        /// are taken into account in the mapping process.
        /// </summary>
        public bool IgnoreMemberMappingResolvedByConvention { get; set; } = false;

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

        public List<IMappingExpressionBuilder> Mappers { get; private set; }

        public MappingConventions Conventions { get; private set; }

        public Configuration( Action<Configuration> config = null )
        {
            var rootMapping = new TypeMapping( this, typeof( object ), typeof( object ) )
            {
                //avoid INHERIT as a value for root-mapping options
                ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE,
                CollectionBehavior = CollectionBehaviors.RESET
            };

            TypeMappingTree = new TypeMappingInheritanceTree( rootMapping );
            _conventionResolver = new DefaultConventionResolver();
            ExpCache = new GeneratedExpressionCache();

            //Order is important: the first MapperExpressionBuilder able to handle a mapping is used.
            //Make sure to use a collection which preserve insertion order!
            this.Mappers = new List<IMappingExpressionBuilder>()
            {
                //new AbstractMappingExpressionBuilder(this),
                new StringToEnumMapper( this ),
                new EnumMapper( this ),
                new BuiltInTypeMapper( this ),
                new NullableMapper( this ),
                new StructMapper( this ),
                new ConvertMapper( this ),
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

            this.Conventions = new MappingConventions( cfg =>
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

            //new BuiltInConversionConverters().OnItself( this );

            config?.Invoke( this );
        }

        public TypeMapping this[ Type source, Type target ]
        {
            get
            {
                var typeMappingNode = this.TypeMappingTree.GetOrAdd( source, target, () =>
                {
                    var newTypeMapping = new TypeMapping( this, source, target );
                    _conventionResolver.MapByConvention( newTypeMapping, this.Conventions );

                    return newTypeMapping;
                } );

                return typeMappingNode.Item;
            }
        }

        public override string ToString()
        {
            return this.TypeMappingTree.ToString();
        }
    }
}
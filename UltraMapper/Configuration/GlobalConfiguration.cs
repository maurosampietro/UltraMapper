using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Config;
using UltraMapper.Conventions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper
{
    public class Configuration
    {
        private readonly TypeMappingInheritanceTree _typeMappings;

        /// <summary>
        /// If set to True only explicitly user-defined member-mappings are 
        /// taken into account in the mapping process.
        /// 
        /// If set to False members-mappings that have been resolved by convention 
        /// are taken into account in the mapping process.
        /// </summary>
        public bool IgnoreMemberMappingResolvedByConvention { get; set; } = false;

        public bool IsReferenceTrackingEnabled { get; set; } = true;

        public CollectionBehaviors CollectionBehavior
        {
            get { return _typeMappings.Root.Item.CollectionBehavior; }
            set { _typeMappings.Root.Item.CollectionBehavior = value; }
        }

        public ReferenceBehaviors ReferenceBehavior
        {
            get { return _typeMappings.Root.Item.ReferenceBehavior; }
            set { _typeMappings.Root.Item.ReferenceBehavior = value; }
        }

        //Order is important: the first MapperExpressionBuilder able to handle a mapping is used.
        //Make sure to use a collection which preserve insertion order!
        public List<IMappingExpressionBuilder> Mappers { get; set; }

        public MappingConventions Conventions { get; set; }

        public Configuration( Action<Configuration> config = null )
        {
            var rootTypePair = new TypePair( typeof( object ), typeof( object ) );
            var rootMapping = new TypeMapping( this, rootTypePair )
            {
                //for the root mapping set values aside from INHERIT for each option
                ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE,
                CollectionBehavior = CollectionBehaviors.RESET
            };

            _typeMappings = new TypeMappingInheritanceTree( rootMapping );

            this.Mappers = new List<IMappingExpressionBuilder>()
            {   
                //new AbstractMappingExpressionBuilder( this ),
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
            typeMapping.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
            typeMapping.CollectionBehavior = CollectionBehaviors.UPDATE;
            typeMapping.CollectionItemEqualityComparer = elementEqualityComparison;

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

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

        private TypeMapping GetTypeMapping( Type source, Type target )
        {
            var typePair = new TypePair( source, target );
            return this.GetTypeMapping( typePair );
        }

        private TypeMapping GetTypeMapping( TypePair typePair )
        {
            var typeMappingNode = _typeMappings.GetOrAdd( typePair, () =>
            {
                var newTypeMapping = new TypeMapping( this, typePair );
                this.MapByConvention( newTypeMapping );

                return newTypeMapping;
            } );

            return typeMappingNode.Item;
            //if( typeMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
            //{
            //    var parentMapping = typeMappingNode.Parent?.Item;
            //    if( parentMapping != null )
            //    {
            //        typeMapping.CollectionBehavior = parentMapping.CollectionBehavior;
            //        typeMapping.CollectionItemEqualityComparer = parentMapping.CollectionItemEqualityComparer;
            //        typeMappingNode.Item.ReferenceBehavior = parentMapping.ReferenceBehavior;
            //    }
            //}
        }

        internal TypeMapping this[ TypePair typePair ]
        {
            get { return this.GetTypeMapping( typePair ); }
        }

        public TypeMapping this[ Type source, Type target ]
        {
            get { return this.GetTypeMapping( source, target ); }
        }

        private void MapByConvention( TypeMapping typeMapping )
        {
            foreach( var convention in this.Conventions )
            {
                var memberPairings = convention.MapByConvention(
                    typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );

                foreach( var memberPair in memberPairings )
                {
                    var sourceMember = memberPair.SourceMemberAccess.Last();
                    var mappingSource = typeMapping.GetMappingSource( sourceMember, memberPair.SourceMemberAccess );

                    var targetMember = memberPair.TargetMemberAccess.Last();
                    var mappingTarget = typeMapping.GetMappingTarget( targetMember, memberPair.TargetMemberAccess );

                    var mapping = new MemberMapping( typeMapping, mappingSource, mappingTarget )
                    {
                        MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION
                    };

                    typeMapping.MemberMappings[ mappingTarget ] = mapping;
                }
            }
        }

        public TypeMapping GetParentConfiguration( TypeMapping typeMapping )
        {
            return _typeMappings[ typeMapping.TypePair ].Parent?.Item;
        }

        public override string ToString()
        {
            return _typeMappings.ToString();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Config;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public sealed class TypeMapping : Mapping, ITypeMappingOptions
    {
        private string _toString;

        public HashSet<IMappingSource> _mappingSource = new HashSet<IMappingSource>();

        //Each source and target member is instantiated only once per typeMapping
        //so we can handle their options/configuration override correctly.
        private readonly Dictionary<MemberInfo, IMappingSource> _sources = new();
        private readonly Dictionary<MemberInfo, IMappingTarget> _targets = new();

        /*
         *A source member can be mapped to multiple target members.
         *
         *A target member can be mapped just once and for that reason 
         *multiple mappings override each other and the last one is used.
         *
         *The target member can be therefore used as the key of this dictionary
         */
        public readonly Dictionary<IMappingTarget, MemberMapping> MemberToMemberMappings;

        public readonly Dictionary<IMappingTarget, Dictionary<Type, MemberMapping>> TypeToMemberMappings;

        public TypeMapping( Configuration config, IMappingSource source, IMappingTarget target )
            : base( config, source, target )
        {
            this.MemberToMemberMappings = new Dictionary<IMappingTarget, MemberMapping>();
            this.TypeToMemberMappings = new Dictionary<IMappingTarget, Dictionary<Type, MemberMapping>>();
        }

        public TypeMapping( Configuration config, Type sourceType, Type targetType )
            : base( config, new MappingSource( sourceType ), new MappingTarget( targetType ) )
        {
            this.MemberToMemberMappings = new Dictionary<IMappingTarget, MemberMapping>();
            this.TypeToMemberMappings = new Dictionary<IMappingTarget, Dictionary<Type, MemberMapping>>();
        }

        public MemberMapping GetTypeToMember( Type sourceType, IMappingTarget targetMember )
        {
            if( TypeToMemberMappings.TryGetValue( targetMember, out var dict ) )
            {
                if( dict.TryGetValue( sourceType, out var typeToMemberMapping ) )
                    return typeToMemberMapping;
            }

            return null;
        }

        private bool? _isReferenceTrackingEnabled = null;
        public bool IsReferenceTrackingEnabled
        {
            get
            {
                if( _isReferenceTrackingEnabled != null )
                    return _isReferenceTrackingEnabled.Value;

                var parent = this.GetParentConfiguration();
                if( parent != null ) return parent.IsReferenceTrackingEnabled;

                return this.GlobalConfig.IsReferenceTrackingEnabled;
            }

            set => _isReferenceTrackingEnabled = value;
        }

        private LambdaExpression _customConverter = null;
        public override LambdaExpression CustomConverter
        {
            get { return _customConverter; }
            set
            {
                _customConverter = CustomConverterExpressionBuilder.ReplaceParams( value );
                //_customConverter = value;

                ////if( TypePair.SourceType.IsBuiltIn( true ) && TypePair.TargetType.IsBuiltIn( true ) )
                ////    _customConverter = CustomConverterExpressionBuilder.ReplaceParams( value );
                ////else
                //_customConverter = CustomConverterExpressionBuilder.Encapsule( value );
            }
        }

        public bool? _ignoreMemberMappingResolvedByConvention = null;
        public bool? IgnoreMemberMappingResolvedByConvention
        {
            get
            {
                if( _ignoreMemberMappingResolvedByConvention != null )
                    return _ignoreMemberMappingResolvedByConvention.Value;

                var parent = this.GetParentConfiguration();
                if( parent != null ) return parent.IgnoreMemberMappingResolvedByConvention;

                return this.GlobalConfig.IgnoreMemberMappingResolvedByConvention;
            }

            set => _ignoreMemberMappingResolvedByConvention = value;
        }

        public ReferenceBehaviors _referenceBehavior = ReferenceBehaviors.INHERIT;
        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                if( _referenceBehavior != ReferenceBehaviors.INHERIT )
                    return _referenceBehavior;

                var parent = this.GetParentConfiguration();
                if( parent != null ) return parent.ReferenceBehavior;

                return this.GlobalConfig.ReferenceBehavior;
            }

            set => _referenceBehavior = value;
        }

        public CollectionBehaviors _collectionBehavior = CollectionBehaviors.INHERIT;
        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                if( _collectionBehavior != CollectionBehaviors.INHERIT )
                    return _collectionBehavior;

                var parent = this.GetParentConfiguration();
                if( parent != null ) return parent.CollectionBehavior;

                return this.GlobalConfig.CollectionBehavior;
            }

            set => _collectionBehavior = value;
        }

        public LambdaExpression _collectionItemEqualityComparer;
        public LambdaExpression CollectionItemEqualityComparer
        {
            get
            {
                if( _collectionItemEqualityComparer != null )
                    return _collectionItemEqualityComparer;

                var parent = this.GetParentConfiguration();
                if( parent != null ) return parent.CollectionItemEqualityComparer;

                return null;
            }

            set => _collectionItemEqualityComparer = value;
        }

        public LambdaExpression _customTargetConstructor;
        public LambdaExpression CustomTargetConstructor
        {
            get => _customTargetConstructor;
            set => _customTargetConstructor = value;
        }

        public void SetCustomTargetConstructor<T>( Expression<Func<T>> ctor )
            => CustomTargetConstructor = ctor;

        public LambdaExpression CustomTargetInsertMethod { get; set; }

        public void SetCustomTargetInsertMethod<TTarget, TItem>( Expression<Action<TTarget, TItem>> insert ) where TTarget : IEnumerable<TItem> =>
                   CustomTargetInsertMethod = insert;

        public void SetCollectionItemEqualityComparer<TSource, TTarget>( Expression<Func<TSource, TTarget, bool>> converter )
            => CollectionItemEqualityComparer = converter;

        private TypeMapping GetParentConfiguration()
        {
            if( this.GlobalConfig.TypeMappingTree.TryGetValue(
                Source.EntryType, Target.EntryType, out ConfigInheritanceNode value ) )
            {
                return value.Parent?.Item;
            }

            return null;
        }

        public IMappingSource GetMappingSource( MemberInfo sourceMember,
            LambdaExpression sourceMemberGetterExpression )
        {
            return _sources.GetOrAdd( sourceMember,
               () =>
               {
                   var mp = new MappingSource( sourceMemberGetterExpression );
                   var isAdded = _mappingSource.Add( mp );

                   return mp;
               } );
        }

        public IMappingTarget GetMappingTarget( MemberInfo targetMember,
            LambdaExpression targetMemberGetter, LambdaExpression targetMemberSetter )
        {
            return _targets.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberSetter, targetMemberGetter ) );
        }

        public IMappingSource GetMappingSource( MemberInfo sourceMember,
            MemberAccessPath sourceMemberPath )
        {
            return _sources.GetOrAdd( sourceMember,
               () =>
               {
                   var mp = new MappingSource( sourceMemberPath );
                   var isAdded = _mappingSource.Add( mp );

                   return mp;
               } );
        }

        public IMappingSource GetNullStructMappingSource( MemberAccessPath sourcePath, LambdaExpression sourceMemberGetterExpression )
        {
            var sourceMember = sourcePath.Last();
            return _sources.GetOrAdd( sourceMember,
                  () =>
                  {
                      var mp = new StructMappingSource( sourcePath, sourceMemberGetterExpression );
                      var isAdded = _mappingSource.Add( mp );
                      return mp;
                  } );
        }

        public IMappingTarget GetMappingTarget( MemberInfo targetMember,
            MemberAccessPath targetMemberPath )
        {
            return _targets.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberPath ) );
        }

        public MemberMapping AddMemberToMemberMapping( IMappingSource source, IMappingTarget target )
        {
            var temp = _mappingSource.FirstOrDefault( item => item == source );
            if( temp != null )
                source = temp;

            var memberMapping = new MemberMapping( this, source, target );
            this.MemberToMemberMappings[ target ] = memberMapping;

            return memberMapping;
        }

        public override string ToString()
        {
            if( _toString == null )
                _toString = $"{this.Source.EntryType} -> {this.Target.EntryType}";

            return _toString;
        }
    }
}

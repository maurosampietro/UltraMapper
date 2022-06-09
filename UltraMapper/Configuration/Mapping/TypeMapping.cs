using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Config;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public sealed class TypeMapping : Mapping, ITypeMappingOptions
    {
        private string _toString;

        //Each source and target member is instantiated only once per typeMapping
        //so we can handle their options/configuration override correctly.
        private readonly Dictionary<MemberInfo, MappingSource> _sourceProperties
            = new Dictionary<MemberInfo, MappingSource>();

        private readonly Dictionary<MemberInfo, MappingTarget> _targetProperties
            = new Dictionary<MemberInfo, MappingTarget>();

        /*
         *A source member can be mapped to multiple target members.
         *
         *A target member can be mapped just once and for that reason 
         *multiple mappings override each other and the last one is used.
         *
         *The target member can be therefore used as the key of this dictionary
         */
        public readonly Dictionary<MappingTarget, MemberMapping> MemberMappings;

        public TypeMapping( Configuration globalConfig, Type sourceType, Type targetType )
            : base( globalConfig, sourceType, targetType )
        {
            this.MemberMappings = new Dictionary<MappingTarget, MemberMapping>();
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

        public void SetCollectionItemEqualityComparer<TSource, TTarget>( Expression<Func<TSource, TTarget, bool>> converter )
            => CollectionItemEqualityComparer = converter;

        private TypeMapping GetParentConfiguration()
        {
            if( this.GlobalConfig.TypeMappingTree.TryGetValue(
                SourceType, TargetType, out ConfigInheritanceNode value ) )
            {
                return value.Parent?.Item;
            }

            return null;
        }

        public MappingSource GetMappingSource( MemberInfo sourceMember,
            LambdaExpression sourceMemberGetterExpression )
        {
            return _sourceProperties.GetOrAdd( sourceMember,
               () => new MappingSource( sourceMemberGetterExpression ) );
        }

        public MappingTarget GetMappingTarget( MemberInfo targetMember,
            LambdaExpression targetMemberGetter, LambdaExpression targetMemberSetter )
        {
            return _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberSetter, targetMemberGetter ) );
        }

        public MappingSource GetMappingSource( MemberInfo sourceMember,
            MemberAccessPath sourceMemberPath )
        {
            return _sourceProperties.GetOrAdd( sourceMember,
                () => new MappingSource( sourceMemberPath ) );
        }

        public MappingTarget GetMappingTarget( MemberInfo targetMember,
            MemberAccessPath targetMemberPath )
        {
            return _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberPath ) );
        }

        public override string ToString()
        {
            if( _toString == null )
                _toString = $"{this.SourceType} -> {this.TargetType}";

            return _toString;
        }
    }
}

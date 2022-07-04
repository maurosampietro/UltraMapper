using System;
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public sealed class MemberMapping : Mapping, IMemberMappingOptions
    {
        private string _toString;

        public readonly TypeMapping InstanceTypeMapping;
        public readonly IMappingSource SourceMember;
        public readonly IMappingTarget TargetMember;

        public MemberMapping( TypeMapping typeMapping, IMappingSource source, IMappingTarget target )
            : base( typeMapping.GlobalConfig, source.MemberType, target.MemberType )
        {
            this.InstanceTypeMapping = typeMapping;

            this.SourceMember = source;
            this.TargetMember = target;
        }

        public bool Ignore { get; set; }

        private bool? _isReferenceTrackingEnabled = null;
        public bool IsReferenceTrackingEnabled
        {
            get
            {
                if( _isReferenceTrackingEnabled != null )
                    return _isReferenceTrackingEnabled.Value;

                if( this.TypeToMemberMapping != null )
                    return this.TypeToMemberMapping.IsReferenceTrackingEnabled;

                return TypeToTypeMapping.IsReferenceTrackingEnabled;
            }

            set => _isReferenceTrackingEnabled = value;
        }

        private TypeMapping _typeToTypeMapping;
        public TypeMapping TypeToTypeMapping
        {
            get
            {
                if( _typeToTypeMapping == null )
                    _typeToTypeMapping = GlobalConfig[ SourceType, TargetType ];

                return _typeToTypeMapping;
            }
        }

        private MemberMapping _typeToMemberMapping;
        public MemberMapping TypeToMemberMapping
        {
            get
            {
                if( _typeToMemberMapping == null )
                {
                    _typeToMemberMapping = this.InstanceTypeMapping
                        .GetTypeToMember( this.SourceType, this.TargetMember );
                }

                return _typeToMemberMapping;
            }
        }

        private ReferenceBehaviors _referenceBehavior = ReferenceBehaviors.INHERIT;
        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                //options resolving:
                //1. user defined manual member option override
                //2. user defined manual type-to-member option override
                //2. user defined manual instance type-to-type option override
                //3. member-type config traversal

                if( _referenceBehavior != ReferenceBehaviors.INHERIT )
                    return _referenceBehavior;

                if( this.TypeToMemberMapping != null && this.TypeToMemberMapping.ReferenceBehavior != ReferenceBehaviors.INHERIT )
                    return this.TypeToMemberMapping.ReferenceBehavior;

                //if( this.InstanceTypeMapping.ReferenceBehavior != ReferenceBehaviors.INHERIT )
                //    return this.InstanceTypeMapping.ReferenceBehavior;

                return TypeToTypeMapping.ReferenceBehavior;
            }

            set => _referenceBehavior = value;
        }

        private CollectionBehaviors _collectionBehaviors = CollectionBehaviors.INHERIT;
        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                //options resolving:
                //1. user defined manual member option override
                //2. user defined manual type-to-member option override
                //2. user defined manual instance type-to-type option override
                //3. member-type config traversal

                if( _collectionBehaviors != CollectionBehaviors.INHERIT )
                    return _collectionBehaviors;

                if( this.TypeToMemberMapping != null && this.TypeToMemberMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.TypeToMemberMapping.CollectionBehavior;

                //in some cases its ok in some cases its not.. do not traver instance type
                //if( this.InstanceTypeMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                //    return this.InstanceTypeMapping.CollectionBehavior;

                return TypeToTypeMapping.CollectionBehavior;
            }

            set => _collectionBehaviors = value;
        }

        private LambdaExpression _collectionItemEqualityComparer;
        public LambdaExpression CollectionItemEqualityComparer
        {
            get
            {
                if( _collectionItemEqualityComparer != null )
                    return _collectionItemEqualityComparer;

                if( this.TypeToMemberMapping?.CollectionItemEqualityComparer != null )
                    return this.TypeToMemberMapping.CollectionItemEqualityComparer;

                return this.TypeToTypeMapping.CollectionItemEqualityComparer;
            }

            set => _collectionItemEqualityComparer = value;
        }

        private LambdaExpression _customConverter;
        public override LambdaExpression CustomConverter
        {
            get
            {
                if( _customConverter != null )
                    return _customConverter;

                if( this.TypeToMemberMapping?.CustomConverter != null )
                    return this.TypeToMemberMapping.CustomConverter;

                return this.TypeToTypeMapping.CustomConverter;
            }

            set => _customConverter = value;
        }

        private LambdaExpression _customTargetConstructor;
        public LambdaExpression CustomTargetConstructor
        {
            get
            {
                if( _customTargetConstructor != null )
                    return _customTargetConstructor;

                if( this.TypeToMemberMapping?.CustomTargetConstructor != null )
                    return this.TypeToMemberMapping.CustomTargetConstructor;

                return this.TypeToTypeMapping.CustomTargetConstructor;
            }

            set => _customTargetConstructor = value;
        }

        public void SetCustomTargetConstructor<T>( Expression<Func<T>> ctor )
            => CustomTargetConstructor = ctor;

        public void SetCollectionItemEqualityComparer<TSource, TTarget>( Expression<Func<TSource, TTarget, bool>> converter )
            => CollectionItemEqualityComparer = converter;

        public override string ToString()
        {
            if( _toString == null )
                _toString = $"{this.SourceMember} -> {this.TargetMember}";

            return _toString;
        }
    }
}
using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public sealed class MemberMapping : Mapping, IMemberMappingOptions
    {
        private string _toString;

        public readonly TypeMapping InstanceTypeMapping;
        public readonly MappingSource SourceMember;
        public readonly MappingTarget TargetMember;

        public MemberMapping( TypeMapping typeMapping, MappingSource source, MappingTarget target )
            : base( typeMapping.GlobalConfig, source.MemberType, target.MemberType )
        {
            this.InstanceTypeMapping = typeMapping;

            this.SourceMember = source;
            this.TargetMember = target;
        }

        public bool Ignore { get; set; }

        private TypeMapping _memberTypeMapping;
        public TypeMapping MemberTypeMapping
        {
            get
            {
                if( _memberTypeMapping == null )
                    _memberTypeMapping = GlobalConfig[ SourceType, TargetType ];

                return _memberTypeMapping;
            }
        }

        private ReferenceBehaviors _referenceBehavior = ReferenceBehaviors.INHERIT;
        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                //options resolving:
                //1. user defined manual member option override
                //2. user defined manual instance option override
                //3. member-type config traversal

                if( _referenceBehavior != ReferenceBehaviors.INHERIT )
                    return _referenceBehavior;

                if( this.InstanceTypeMapping.ReferenceBehavior != ReferenceBehaviors.INHERIT )
                    return this.InstanceTypeMapping.ReferenceBehavior;

                return MemberTypeMapping.ReferenceBehavior;
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
                //2. user defined manual instance option override
                //3. member-type config traversal

                if( _collectionBehaviors != CollectionBehaviors.INHERIT )
                    return _collectionBehaviors;
                
                //architecturally not ready for this
                //if( this.InstanceTypeMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                //    return this.InstanceTypeMapping.CollectionBehavior;

                return MemberTypeMapping.CollectionBehavior;
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

                return this.MemberTypeMapping.CollectionItemEqualityComparer;
            }

            set => _collectionItemEqualityComparer = value;
        }

        private LambdaExpression _customConverter;
        public override LambdaExpression CustomConverter
        {
            get => _customConverter ?? this.MemberTypeMapping.CustomConverter;
            set => _customConverter = value;
        }

        private LambdaExpression _customTargetConstructor;
        public LambdaExpression CustomTargetConstructor
        {
            get => _customTargetConstructor ?? this.MemberTypeMapping.CustomTargetConstructor;
            set => _customTargetConstructor = value;
        }

        public override string ToString()
        {
            if( _toString == null )
                _toString = $"{this.SourceMember} -> {this.TargetMember}";

            return _toString;
        }
    }
}

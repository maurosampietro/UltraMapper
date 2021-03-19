using System.Linq.Expressions;

namespace UltraMapper.Internals
{
    public class MemberMapping : BaseMapping, IMemberOptions
    {
        private string _toString;
        
        public readonly TypeMapping InstanceTypeMapping;
        public readonly MappingSource SourceMember;
        public readonly MappingTarget TargetMember;

        public MemberMapping( TypeMapping typeMapping,
            MappingSource sourceMember, MappingTarget targetMember )
            : base( typeMapping.GlobalConfig,
                 new TypePair( sourceMember.MemberType, targetMember.MemberType ) )
        {
            this.InstanceTypeMapping = typeMapping;

            this.SourceMember = sourceMember;
            this.TargetMember = targetMember;
        }

        public bool Ignore { get; set; }

        private TypeMapping _memberTypeMapping;
        public TypeMapping MemberTypeMapping
        {
            get
            {
                if( _memberTypeMapping == null )
                {
                    _memberTypeMapping = 
                        GlobalConfig[ SourceMember.MemberType, TargetMember.MemberType ];
                }

                return _memberTypeMapping;
            }
        }

        private LambdaExpression _customConverter;
        public override LambdaExpression CustomConverter
        {
            get { return _customConverter ?? MemberTypeMapping.CustomConverter; }
            set { _customConverter = value; }
        }

        private LambdaExpression _customTargetConstructor;
        public LambdaExpression CustomTargetConstructor
        {
            get { return _customTargetConstructor ?? MemberTypeMapping.CustomTargetConstructor; }
            set { _customTargetConstructor = value; }
        }
        
        public LambdaExpression CollectionItemEqualityComparer { get; set; }
        
        public CollectionBehaviors CollectionBehavior { get; set; } 
            = CollectionBehaviors.INHERIT;

        public ReferenceBehaviors ReferenceBehavior { get; set; } 
            = ReferenceBehaviors.INHERIT;

        public override string ToString()
        {
            if( _toString == null )
                _toString = $"{this.SourceMember} -> {this.TargetMember}";

            return _toString;
        }
    }
}

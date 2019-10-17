using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals
{
    public class MemberMappingCrawler : IMemberOptions
    {
        public readonly MemberMapping MemberMapping;
        private readonly TypeMappingCrawler MemberMappingTypeCrawler;
            
        public MemberMappingCrawler( MemberMapping memberMapping )
        {
            this.MemberMapping = memberMapping;
            this.MemberMappingTypeCrawler = new TypeMappingCrawler( this.MemberMapping.MemberTypeMapping );
        }

        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                //options resolving:
                //1. user defined manual member option override
                //2. user defined manual instance option override
                //3. member type config

                if( this.MemberMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.MemberMapping.CollectionBehavior;

                if( this.MemberMapping.InstanceTypeMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.MemberMapping.InstanceTypeMapping.CollectionBehavior;

                return MemberMappingTypeCrawler.CollectionBehavior;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Ignore
        {
            get => this.MemberMapping.Ignore;
            set => throw new NotImplementedException();
        }

        public ReferenceBehaviors ReferenceBehavior
        {
            get => this.MemberMapping.ReferenceBehavior;
            set => throw new NotImplementedException();
        }

        public LambdaExpression CollectionItemEqualityComparer
        {
            get => this.MemberMapping.CollectionItemEqualityComparer;
            set => throw new NotImplementedException();
        }

        public LambdaExpression CustomTargetConstructor
        {
            get => this.MemberMapping.CustomTargetConstructor;
            set => throw new NotImplementedException();
        }
    }

    public class TypeMappingCrawler : ITypeOptions
    {
        public readonly TypeMapping TypeMapping;

        public TypeMappingCrawler( TypeMapping typeMapping )
        {
            this.TypeMapping = typeMapping;
        }

        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                if( this.TypeMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.TypeMapping.CollectionBehavior;

                var parent = this.TypeMapping.GlobalConfiguration.GetParentConfiguration( this.TypeMapping );
                if( parent != null ) return parent.CollectionBehavior;

                return this.TypeMapping.GlobalConfiguration.CollectionBehavior;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IgnoreMemberMappingResolvedByConvention
        {
            get => this.TypeMapping.IgnoreMemberMappingResolvedByConvention;
            set => throw new NotImplementedException();
        }

        public ReferenceBehaviors ReferenceBehavior
        {
            get => this.TypeMapping.ReferenceBehavior;
            set => throw new NotImplementedException();
        }

        public LambdaExpression CollectionItemEqualityComparer
        {
            get => this.TypeMapping.CollectionItemEqualityComparer;
            set => throw new NotImplementedException();
        }

        public LambdaExpression CustomTargetConstructor
        {
            get => this.TypeMapping.CustomTargetConstructor;
            set => throw new NotImplementedException();
        }
    }
}

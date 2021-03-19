using System;
using System.Linq.Expressions;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public class MemberMappingOptionsInheritanceTraversal : IMapping, IMemberOptions
    {
        public readonly MemberMapping MemberMapping;
        private readonly TypeMappingOptionsInheritanceTraversal MemberMappingTypeCrawler;

        public MemberMappingOptionsInheritanceTraversal( MemberMapping memberMapping )
        {
            this.MemberMapping = memberMapping;
            this.MemberMappingTypeCrawler = new TypeMappingOptionsInheritanceTraversal( this.MemberMapping.MemberTypeMapping );
        }

        public Action<ReferenceTracker, object, object> MappingFunc => this.MemberMapping.MappingFunc;
        public LambdaExpression MappingExpression => this.MemberMapping.MappingExpression;

        public bool Ignore
        {
            get => this.MemberMapping.Ignore;
            set => throw new NotImplementedException();
        }

        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                //options resolving:
                //1. user defined manual member option override
                //2. user defined manual instance option override
                //3. member-type config traversal

                if( this.MemberMapping.ReferenceBehavior != ReferenceBehaviors.INHERIT )
                    return this.MemberMapping.ReferenceBehavior;

                if( this.MemberMapping.InstanceTypeMapping.ReferenceBehavior != ReferenceBehaviors.INHERIT )
                    return this.MemberMapping.InstanceTypeMapping.ReferenceBehavior;

                return MemberMappingTypeCrawler.ReferenceBehavior;
            }

            set => throw new NotImplementedException();
        }

        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                //options resolving:
                //1. user defined manual member option override
                //2. user defined manual instance option override
                //3. member-type config traversal

                if( this.MemberMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.MemberMapping.CollectionBehavior;

                if( this.MemberMapping.InstanceTypeMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.MemberMapping.InstanceTypeMapping.CollectionBehavior;

                return MemberMappingTypeCrawler.CollectionBehavior;
            }

            set => throw new NotImplementedException();
        }

        public LambdaExpression CollectionItemEqualityComparer
        {
            get
            {
                if( MemberMapping.CollectionItemEqualityComparer != null )
                    return MemberMapping.CollectionItemEqualityComparer;

                return this.MemberMappingTypeCrawler.CollectionItemEqualityComparer;
            }

            set => throw new NotImplementedException();
        }

        public LambdaExpression CustomTargetConstructor
        {
            get => this.MemberMapping.CustomTargetConstructor;
            set => throw new NotImplementedException();
        }

        public IMappingExpressionBuilder Mapper => this.MemberMapping.Mapper;
    }

    public class TypeMappingOptionsInheritanceTraversal : IMapping, ITypeOptions
    {
        public readonly TypeMapping TypeMapping;

        public TypeMappingOptionsInheritanceTraversal( TypeMapping typeMapping )
        {
            this.TypeMapping = typeMapping;
        }

        public Action<ReferenceTracker, object, object> MappingFunc => this.TypeMapping.MappingFunc;
        public LambdaExpression MappingExpression => this.TypeMapping.MappingExpression;

        public bool? IgnoreMemberMappingResolvedByConvention
        {
            get
            {
                if( this.TypeMapping.IgnoreMemberMappingResolvedByConvention != null )
                    return this.TypeMapping.IgnoreMemberMappingResolvedByConvention;

                var parent = TypeMapping.GlobalConfig.GetParentConfiguration( this.TypeMapping );
                if( parent != null ) return parent.IgnoreMemberMappingResolvedByConvention;

                return this.TypeMapping.GlobalConfig.IgnoreMemberMappingResolvedByConvention;
            }

            set => throw new NotImplementedException();
        }

        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                if( this.TypeMapping.ReferenceBehavior != ReferenceBehaviors.INHERIT )
                    return this.TypeMapping.ReferenceBehavior;

                var parent = this.TypeMapping.GlobalConfig.GetParentConfiguration( this.TypeMapping );
                if( parent != null ) return parent.ReferenceBehavior;

                return this.TypeMapping.GlobalConfig.ReferenceBehavior;
            }

            set => throw new NotImplementedException();
        }

        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                if( this.TypeMapping.CollectionBehavior != CollectionBehaviors.INHERIT )
                    return this.TypeMapping.CollectionBehavior;

                var parent = this.TypeMapping.GlobalConfig.GetParentConfiguration( this.TypeMapping );
                if( parent != null ) return parent.CollectionBehavior;

                return this.TypeMapping.GlobalConfig.CollectionBehavior;
            }

            set => throw new NotImplementedException();
        }

        public LambdaExpression CollectionItemEqualityComparer
        {
            get
            {
                if( this.TypeMapping.CollectionItemEqualityComparer != null )
                    return this.TypeMapping.CollectionItemEqualityComparer;

                var parent = this.TypeMapping.GlobalConfig.GetParentConfiguration( this.TypeMapping );
                if( parent != null ) return parent.CollectionItemEqualityComparer;

                return null;
            }

            set => throw new NotImplementedException();
        }

        public LambdaExpression CustomTargetConstructor
        {
            get => this.TypeMapping.CustomTargetConstructor;
            set => throw new NotImplementedException();
        }

        public IMappingExpressionBuilder Mapper => this.TypeMapping.Mapper;
    }
}

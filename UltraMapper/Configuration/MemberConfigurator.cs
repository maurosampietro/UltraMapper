using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper
{
    public class MemberConfigurator
    {
        protected readonly TypeMapping _typeMapping;

        public MemberConfigurator( TypeMapping typeMapping )
        {
            _typeMapping = typeMapping;
        }

        public MemberConfigurator MapMember( MemberInfo sourceMember,
            MemberInfo targetMemberGetter, MemberInfo targetMemberSetter )
        {
            var sourceMemberGetter = sourceMember.GetGetterLambdaExpression();
            var targetMemberGetterExp = targetMemberGetter.GetGetterLambdaExpression();
            var targetMemberSetterExp = targetMemberSetter.GetSetterLambdaExpression();

            MemberMapping mapping = this.MapMemberInternal( sourceMember, targetMemberGetter,
                sourceMemberGetter, targetMemberGetterExp, targetMemberSetterExp );

            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        public MemberConfigurator MapMember( MemberInfo sourceMember, MemberInfo targetMember )
        {
            var sourceMemberGetter = sourceMember.GetGetterLambdaExpression();
            var targetMemberGetter = targetMember.GetGetterLambdaExpression();
            var targetMemberSetter = targetMember.GetSetterLambdaExpression();

            MemberMapping mapping = this.MapMemberInternal( sourceMember, targetMember,
                sourceMemberGetter, targetMemberGetter, targetMemberSetter );

            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        protected MemberMapping MapMemberInternal( LambdaExpression sourceMemberGetter,
            LambdaExpression targetMemberGetter, LambdaExpression targetMemberSetter )
        {
            var sourceMember = sourceMemberGetter.GetMemberAccessPath().Last();
            var targetMember = targetMemberGetter.GetMemberAccessPath().Last();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetter,
                targetMemberGetter, targetMemberSetter );
        }

        protected MemberMapping MapMemberInternal( LambdaExpression sourceMemberGetter,
            LambdaExpression targetMemberGetter )
        {
            var sourceMember = sourceMemberGetter.GetMemberAccessPath().Last();
            var targetMember = targetMemberGetter.GetMemberAccessPath().Last();

            var targetMemberSetterExpression = targetMemberGetter.GetMemberAccessPath().GetSetterLambdaExpression();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetter,
                targetMemberGetter, targetMemberSetterExpression );
        }

        protected MemberMapping MapMemberInternal( MemberInfo sourceMember, MemberInfo targetMember,
            LambdaExpression sourceMemberGetter, LambdaExpression targetMemberGetter,
            LambdaExpression targetMemberSetter )
        {
            var mappingSource = _typeMapping.GetMappingSource( sourceMember,
                sourceMemberGetter );

            var mappingTarget = _typeMapping.GetMappingTarget( targetMember,
                targetMemberGetter, targetMemberSetter );

            var mapping = new MemberMapping( _typeMapping, mappingSource, mappingTarget );
            _typeMapping.MemberMappings[ mappingTarget ] = mapping;

            return mapping;
        }
    }

    public class MemberConfigurator<TSource, TTarget> : MemberConfigurator
    {
        public MemberConfigurator( TypeMapping typeMapping )
            : base( typeMapping ) { }

        public MemberConfigurator<TSource, TTarget> IgnoreSourceMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector )
        {
            var sourceMemberAcessPath = sourceMemberSelector.GetMemberAccessPath();
            var sourceMember = sourceMemberAcessPath.Last();

            var mappingSource = _typeMapping.GetMappingSource(
                sourceMember, sourceMemberAcessPath );

            mappingSource.Ignore = true;
            return this;
        }

        public MemberConfigurator<TSource, TTarget> IgnoreTargetMember<TTargetMember>(
            Expression<Func<TTarget, TTargetMember>> targetMemberSelector )
        {
            var targetMemberAccessPath = targetMemberSelector.GetMemberAccessPath();
            var targetMember = targetMemberAccessPath.Last();
            var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

            var mappingTarget = _typeMapping.GetMappingTarget( targetMember,
                targetMemberSelector, targetMemberSetterExpression );

            mappingTarget.Ignore = true;
            return this;
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector,
            Expression<Func<TTarget, TTargetMember>> targetMemberGetter,
            Expression<Action<TTarget, TSourceMember>> targetMemberSetter,
            Action<IMemberOptions> memberMappingConfig = null )
        {
            var mapping = base.MapMemberInternal( sourceMemberSelector,
                targetMemberGetter, targetMemberSetter );

            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            memberMappingConfig?.Invoke( mapping );

            return this;
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector )
        {
            return MapMember( sourceSelector, targetSelector, null, null );
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Expression<Func<TSourceMember, TTargetMember>> converter )
        {
            return MapMember( sourceSelector, targetSelector, converter, null );
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Action<IMemberOptions> memberMappingConfig )
        {
            return MapMember( sourceSelector, targetSelector, null, memberMappingConfig );
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Expression<Func<TSourceMember, TTargetMember>> converter,
            Action<IMemberOptions> memberMappingConfig )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            mapping.CustomConverter = converter;
            memberMappingConfig?.Invoke( mapping );

            return this;
        }

        //COLLECTION OVERLOADS

        /// <summary>
        /// Map a member implementing IEnumerable to another IEnumerable for updating.
        /// Each element matching the equality comparison rule is updated.
        /// Each element that exists in source but is missing in target is added.
        /// </summary>
        /// <typeparam name="TSourceMember"></typeparam>
        /// <typeparam name="TTargetMember"></typeparam>
        /// <param name="sourceSelector"></param>
        /// <param name="targetSelector"></param>
        /// <param name="elementEqualityComparer"></param>
        /// <returns></returns>
        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceSelector,
            Expression<Func<TTarget, IEnumerable<TTargetMember>>> targetSelector,
            Expression<Func<TSourceMember, TTargetMember, bool>> elementEqualityComparer )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            mapping.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
            mapping.CollectionBehavior = CollectionBehaviors.UPDATE;
            mapping.CollectionItemEqualityComparer = elementEqualityComparer;

            return this;
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceSelector,
            Expression<Func<TTarget, IEnumerable<TTargetMember>>> targetSelector,
            Action<IMemberOptions> memberMappingConfig = null )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            memberMappingConfig.Invoke( mapping );

            return this;
        }
    }
}

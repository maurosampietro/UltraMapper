using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Conventions;

namespace UltraMapper
{
    public class MemberConfigurator
    {
        protected readonly TypeMapping _typeMapping;

        public MemberConfigurator( TypeMapping typeMapping )
        {
            _typeMapping = typeMapping;
        }

        public MemberConfigurator MapMember( MemberInfo sourceMember, MemberInfo targetMember )
        {
            var sourceMemberGetterExpression = sourceMember.GetGetterLambdaExpression();
            var targetMemberGetterExpression = targetMember.GetGetterLambdaExpression();
            var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

            MemberMapping mapping = this.MapMemberInternal( sourceMember, targetMember, 
                sourceMemberGetterExpression, targetMemberGetterExpression, targetMemberSetterExpression );

            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        protected MemberMapping MapMemberInternal( LambdaExpression sourceMemberGetterExpression,
            LambdaExpression targetMemberGetterExpression, LambdaExpression targetMemberSetterExpression )
        {
            var sourceMember = sourceMemberGetterExpression.ExtractMember();
            var targetMember = targetMemberGetterExpression.ExtractMember();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetterExpression,
                targetMemberGetterExpression, targetMemberSetterExpression );
        }

        protected MemberMapping MapMemberInternal( LambdaExpression sourceMemberGetterExpression,
            LambdaExpression targetMemberGetterExpression )
        {
            var sourceMember = sourceMemberGetterExpression.ExtractMember();
            var targetMember = targetMemberGetterExpression.ExtractMember();

            var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetterExpression,
                targetMemberGetterExpression, targetMemberSetterExpression );
        }

        protected MemberMapping MapMemberInternal( MemberInfo sourceMember, MemberInfo targetMember,
            LambdaExpression sourceMemberGetterExpression, LambdaExpression targetMemberGetterExpression,
            LambdaExpression targetMemberSetterExpression )
        {
            var mappingSource = _typeMapping.GetMappingSource( sourceMember,
                sourceMemberGetterExpression );

            var mappingTarget = _typeMapping.GetMappingTarget( targetMember,
                targetMemberGetterExpression, targetMemberSetterExpression );

            var mapping = _typeMapping.GetMemberMapping( mappingSource, mappingTarget );
            _typeMapping.MemberMappings[ mappingTarget ] = mapping;

            return mapping;
        }
    }

    public class MemberConfigurator<TSource, TTarget> : MemberConfigurator
    {
        public MemberConfigurator( TypeMapping typeMapping ) : base( typeMapping ) { }

        public MemberConfigurator<TSource, TTarget> IgnoreSourceMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector,
            params Expression<Func<TSource, TSourceMember>>[] sourceMemberSelectors )
        {
            var selectors = new[] { sourceMemberSelector }
                .Concat( sourceMemberSelectors );

            foreach( var selector in selectors )
            {
                var mappingSource = _typeMapping.GetMappingSource(
                    selector.ExtractMember(), selector );

                mappingSource.Ignore = true;
            }

            return this;
        }

        public MemberConfigurator<TSource, TTarget> IgnoreTargetMember<TTargetMember>(
            Expression<Func<TSource, TTargetMember>> targetMemberSelector,
            params Expression<Func<TSource, TTargetMember>>[] targetMemberSelectors )
        {
            var selectors = new[] { targetMemberSelector }
                .Concat( targetMemberSelectors );

            foreach( var selector in selectors )
            {
                var targetMember = selector.ExtractMember();
                var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

                var mappingTarget = _typeMapping.GetMappingTarget( targetMember,
                    selector, targetMemberSetterExpression );

                mappingTarget.Ignore = true;
            }

            return this;
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector,
            Expression<Func<TTarget, TTargetMember>> targetMemberGetter,
            Expression<Action<TTarget, TSourceMember>> targetMemberSetter,
            Action<IMemberOptions> memberMappingConfig = null )
        {
            var mapping = base.MapMemberInternal( sourceMemberSelector, targetMemberGetter, targetMemberSetter );
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
            mapping.CollectionMappingStrategy = CollectionMappingStrategies.UPDATE;
            mapping.CollectionEqualityComparer = elementEqualityComparer;

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

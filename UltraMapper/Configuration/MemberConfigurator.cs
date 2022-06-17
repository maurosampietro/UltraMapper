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

        public MemberConfigurator MapMember( Type sourceType, MemberInfo targetMember )
        {
            var sourceMemberGetter = sourceType.GetGetterExp();
            var targetMemberGetterExp = targetMember.GetGetterExp();
            var targetMemberSetterExp = targetMember.GetSetterExp();

            MemberMapping mapping = this.MapMemberInternal( sourceType, targetMember,
                sourceMemberGetter, targetMemberGetterExp, targetMemberSetterExp );

            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        public MemberConfigurator MapMember( MemberInfo sourceMember,
            MemberInfo targetMemberGetter, MemberInfo targetMemberSetter )
        {
            var sourceMemberGetter = sourceMember.GetGetterExp();
            var targetMemberGetterExp = targetMemberGetter.GetGetterExp();
            var targetMemberSetterExp = targetMemberSetter.GetSetterExp();

            MemberMapping mapping = this.MapMemberInternal( sourceMember, targetMemberGetter,
                sourceMemberGetter, targetMemberGetterExp, targetMemberSetterExp );

            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        public MemberConfigurator MapMember( MemberInfo sourceMember, MemberInfo targetMember )
        {
            var sourceMemberGetter = sourceMember.GetGetterExp();
            var targetMemberGetter = targetMember.GetGetterExp();
            var targetMemberSetter = targetMember.GetSetterExp();

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
            var targetMemberPath = targetMemberGetter.GetMemberAccessPath();
            var targetMember = targetMemberPath.Last();

            var targetMemberSetterExpression = targetMemberPath.GetSetterExp();

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

    //Member to member
    public partial class MemberConfigurator<TSource, TTarget> : MemberConfigurator
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
            var targetMemberSetterExpression = targetMember.GetSetterExp();

            var mappingTarget = _typeMapping.GetMappingTarget( targetMember,
                targetMemberSelector, targetMemberSetterExpression );

            mappingTarget.Ignore = true;
            return this;
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector,
            Expression<Func<TTarget, TTargetMember>> targetMemberGetter,
            Expression<Action<TTarget, TSourceMember>> targetMemberSetter,
            Action<IMemberMappingOptions> memberMappingConfig = null )
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
            return MapMember( sourceSelector, targetSelector, (Expression<Func<TSourceMember, TTargetMember>>)null, null );
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
          Expression<Func<TSource, TSourceMember>> sourceSelector,
          Expression<Func<TTarget, TTargetMember>> targetSelector,
          Expression<Func<ReferenceTracker, TSourceMember, TTargetMember>> converter )
        {
            return MapMember( sourceSelector, targetSelector, converter, null );
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
            Action<IMemberMappingOptions> memberMappingConfig )
        {
            return MapMember( sourceSelector, targetSelector,
                (Expression<Func<TSourceMember, TTargetMember>>)null, memberMappingConfig );
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Expression<Func<ReferenceTracker, TSourceMember, TTargetMember>> converter,
            Action<IMemberMappingOptions> memberMappingConfig )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            mapping.CustomConverter = converter;
            memberMappingConfig?.Invoke( mapping );

            return this;

            // return MapMember( sourceSelector, targetSelector, converter, memberMappingConfig );
        }

        public MemberConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Expression<Func<TSourceMember, TTargetMember>> converter,
            Action<IMemberMappingOptions> memberMappingConfig )
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
            Action<IMemberMappingOptions> memberMappingConfig = null )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            memberMappingConfig.Invoke( mapping );

            return this;
        }

        //type to member
        public MemberConfigurator MapTypeToMember<TType, TTargetMember>(
            Expression<Func<TTarget, TTargetMember>> targetSelector )
        {
            var sourceSelector = typeof( TType ).GetGetterExp();
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        public MemberConfigurator MapTypeToMember<TTargetMember>( Type type,
            Expression<Func<TTarget, TTargetMember>> targetSelector )
        {
            var sourceSelector = type.GetGetterExp();
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        public MemberConfigurator MapTypeToMember<TType>( MemberInfo targetMember )
        {
            var sourceSelector = typeof( TType ).GetGetterExp();
            var targetSelector = targetMember.GetSetterExp();

            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }
    }
}

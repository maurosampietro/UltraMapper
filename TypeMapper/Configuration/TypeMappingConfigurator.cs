using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.ExtensionMethods;
using TypeMapper.Internals;

namespace TypeMapper.Configuration
{
    public class TypeMappingConfigurator
    {
        //Each source and target member is instantiated only once per configuration
        //so we can handle their options/configuration override correctly.
        protected readonly Dictionary<MemberInfo, MappingSource> _sourceProperties
            = new Dictionary<MemberInfo, MappingSource>();

        protected readonly Dictionary<MemberInfo, MappingTarget> _targetProperties
            = new Dictionary<MemberInfo, MappingTarget>();

        protected readonly TypeMapping _typeMapping;
        protected readonly GlobalConfiguration _globalConfiguration;

        public TypeMappingConfigurator( TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration )
        {
            _typeMapping = typeMapping;
            _globalConfiguration = globalConfiguration;

            //if( !typeMapping.IgnoreMappingResolveByConvention )
            this.MapByConvention( typeMapping );
        }

        public TypeMappingConfigurator( TypePair typePair, GlobalConfiguration globalConfiguration )
            : this( new TypeMapping( globalConfiguration, typePair ), globalConfiguration ) { }

        public TypeMappingConfigurator( Type sourceType, Type targetType, GlobalConfiguration globalConfiguration )
            : this( new TypeMapping( globalConfiguration, new TypePair( sourceType, targetType ) ), globalConfiguration ) { }

        public TypeMappingConfigurator MapMember( MemberInfo sourceMember, MemberInfo targetMember )
        {
            MemberMapping mapping = MapMemberInternal( sourceMember, targetMember );
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

        protected MemberMapping MapMemberInternal( MemberInfo sourceMember, MemberInfo targetMember )
        {
            var sourceMemberGetterExpression = sourceMember.GetGetterLambdaExpression();
            var targetMemberGetterExpression = targetMember.GetGetterLambdaExpression();
            var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetterExpression,
                targetMemberGetterExpression, targetMemberSetterExpression );
        }

        protected MemberMapping MapMemberInternal( MemberInfo sourceMember, MemberInfo targetMember,
            LambdaExpression sourceMemberGetterExpression, LambdaExpression targetMemberGetterExpression,
            LambdaExpression targetMemberSetterExpression )
        {
            var mappingSource = _sourceProperties.GetOrAdd( sourceMember,
                () => new MappingSource( sourceMemberGetterExpression ) );

            var mappingTarget = _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberGetterExpression, targetMemberSetterExpression ) );

            var mapping = new MemberMapping( _typeMapping, mappingSource, mappingTarget );
            _typeMapping.MemberMappings[ targetMember ] = mapping;

            return mapping;
        }

        protected void MapByConvention( TypeMapping typeMapping )
        {
            var source = typeMapping.TypePair.SourceType;
            var target = typeMapping.TypePair.TargetType;

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;

            var sourceProperties = source.GetProperties( bindingAttributes )
                .Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

            var targetProperties = target.GetProperties( bindingAttributes )
                .Where( p => p.CanWrite && p.GetSetMethod() != null &&
                    p.GetIndexParameters().Length == 0 ); //no indexed properties

            var sourceFields = source.GetFields( bindingAttributes );
            var targetFields = target.GetFields( bindingAttributes );

            var sourceMembers = sourceProperties.Cast<MemberInfo>().Concat( sourceFields );
            var targetMembers = targetProperties.Cast<MemberInfo>().Concat( targetFields );

            foreach( var sourceMember in sourceMembers )
            {
                foreach( var targetMember in targetMembers )
                {
                    if( _globalConfiguration.MappingConvention.IsMatch( sourceMember, targetMember ) )
                    {
                        var mapping = this.MapMemberInternal( sourceMember, targetMember );
                        mapping.MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION;

                        break; //sourceMember is now mapped, jump directly to the next sourceMember
                    }
                }
            }
        }
    }

    public class TypeMappingConfigurator<TSource, TTarget> : TypeMappingConfigurator
    {
        public TypeMappingConfigurator( GlobalConfiguration globalConfiguration )
            : base( typeof( TSource ), typeof( TTarget ), globalConfiguration ) { }

        public TypeMappingConfigurator( TypeMapping typeMapping, GlobalConfiguration globalConfiguration )
            : base( typeMapping, globalConfiguration ) { }

        public TypeMappingConfigurator<TSource, TTarget> IgnoreSourceMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector,
            params Expression<Func<TSource, TSourceMember>>[] sourceMemberSelectors )
        {
            var selectors = new[] { sourceMemberSelector }
                .Concat( sourceMemberSelectors );

            foreach( var selector in selectors )
            {
                var mappingSource = _sourceProperties.GetOrAdd(
                    selector.ExtractMember(), () => new MappingSource( selector ) );

                mappingSource.Ignore = true;
            }

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> IgnoreTargetMember<TTargetMember>(
            Expression<Func<TSource, TTargetMember>> targetMemberSelector,
            params Expression<Func<TSource, TTargetMember>>[] targetMemberSelectors )
        {
            var selectors = new[] { targetMemberSelector }
                .Concat( targetMemberSelectors );

            foreach( var selector in selectors )
            {
                var targetMember = selector.ExtractMember();
                var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

                var mappingTarget = _targetProperties.GetOrAdd(
                    targetMember, () => new MappingTarget( selector, targetMemberSetterExpression ) );

                mappingTarget.Ignore = true;
            }

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceMemberSelector,
            Expression<Func<TTarget, TTargetMember>> targetMemberGetter,
            Expression<Action<TTarget, TSourceMember>> targetMemberSetter )
        {
            var mapping = base.MapMemberInternal( sourceMemberSelector, targetMemberGetter, targetMemberSetter );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Expression<Func<TSourceMember, TTargetMember>> converter = null )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            mapping.CustomConverter = converter;

            return this;
        }

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
        public TypeMappingConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
           Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceSelector,
           Expression<Func<TTarget, IEnumerable<TTargetMember>>> targetSelector,
           Expression<Func<TSourceMember, TTargetMember, bool>> elementEqualityComparer )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            mapping.CollectionEqualityComparer = elementEqualityComparer;

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
         Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceSelector,
         Expression<Func<TTarget, IEnumerable<TTargetMember>>> targetSelector,
         Action<IMappingOptions> options )
        {
            var mapping = base.MapMemberInternal( sourceSelector, targetSelector );
            mapping.MappingResolution = MappingResolution.USER_DEFINED;
            options.Invoke( mapping );

            return this;
        }
    }
}

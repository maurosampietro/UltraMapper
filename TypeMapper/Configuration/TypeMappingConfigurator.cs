using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

            if( !typeMapping.IgnoreConventions )
                this.MapByConvention( typeMapping );
        }

        public TypeMappingConfigurator( TypePair typePair, GlobalConfiguration globalConfiguration )
            : this( new TypeMapping( globalConfiguration, typePair ), globalConfiguration ) { }

        public TypeMappingConfigurator( Type sourceType, Type targetType, GlobalConfiguration globalConfiguration )
            : this( new TypeMapping( globalConfiguration, new TypePair( sourceType, targetType ) ), globalConfiguration ) { }

        public TypeMappingConfigurator MapMember( LambdaExpression sourceMemberGetterExpression,
         LambdaExpression targetMemberGetterExpression, LambdaExpression converter = null )
        {
            var sourceMember = sourceMemberGetterExpression.ExtractMember();
            var targetMember = targetMemberGetterExpression.ExtractMember();

            var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetterExpression,
                   targetMemberGetterExpression, targetMemberSetterExpression, MappingResolution.USER_DEFINED, converter );
        }

        public TypeMappingConfigurator MapMember( MemberInfo sourceMember, MemberInfo targetMember )
        {
            var sourceMemberGetterExpression = sourceMember.GetGetterLambdaExpression();
            var targetMemberGetterExpression = targetMember.GetGetterLambdaExpression();
            var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

            return this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetterExpression,
                targetMemberGetterExpression, targetMemberSetterExpression, MappingResolution.USER_DEFINED, null );
        }

        private TypeMappingConfigurator MapMemberInternal( MemberInfo sourceMember, MemberInfo targetMember,
            LambdaExpression sourceMemberGetterExpression, LambdaExpression targetMemberGetterExpression,
            LambdaExpression targetMemberSetterExpression, MappingResolution mappingResolution, LambdaExpression converter )
        {
            var mappingSource = _sourceProperties.GetOrAdd( sourceMember,
                () => new MappingSource( sourceMemberGetterExpression ) );

            var mappingTarget = _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberGetterExpression, targetMemberSetterExpression ) );

            var mapping = new MemberMapping( _typeMapping, mappingSource, mappingTarget )
            {
                CustomConverter = converter,
                MappingResolution = mappingResolution,
            };

            _typeMapping.MemberMappings.UpdateOrAdd( targetMember, mapping );

            return this;
        }

        protected internal void MapByConvention( TypeMapping typeMapping )
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
                        var sourceMemberGetterExpression = sourceMember.GetGetterLambdaExpression();
                        var targetMemberGetterExpression = targetMember.GetGetterLambdaExpression();
                        var targetMemberSetterExpression = targetMember.GetSetterLambdaExpression();

                        this.MapMemberInternal( sourceMember, targetMember, sourceMemberGetterExpression,
                            targetMemberGetterExpression, targetMemberSetterExpression,
                            MappingResolution.RESOLVED_BY_CONVENTION, null );

                        break; //sourceProperty is now mapped, jump directly to the next sourceProperty
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

        public TypeMappingConfigurator<TSource, TTarget> MapMember<TSourceMember>(
            Expression<Func<TSource, TSourceMember>> sourcePropertySelector,
            Expression<Action<TTarget, TSourceMember>> targetPropertySelector )
        {
            return (TypeMappingConfigurator<TSource, TTarget>)
                base.MapMember( sourcePropertySelector, targetPropertySelector, null );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapMember<TSourceMember, TTargetMember>(
            Expression<Func<TSource, TSourceMember>> sourceSelector,
            Expression<Func<TTarget, TTargetMember>> targetSelector,
            Expression<Func<TSourceMember, TTargetMember>> converter = null )
        {
            return (TypeMappingConfigurator<TSource, TTarget>)
                base.MapMember( sourceSelector, targetSelector, converter );
        }

        ////source instance directly to property.
        //public TypeMappingConfigurator<TSource, TTarget> MapProperty<TTargetProperty>(
        //       Expression<Func<TTarget, TTargetProperty>> sourcePropertySelector,
        //       Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
        //       Expression<Func<TSource,TTargetProperty>> converter = null )
        //{
        //    var targetMemberInfo = targetPropertySelector.ExtractProperty();
        //    return this.MapProperty( null, targetMemberInfo, converter );
        //}

        //public TypeMappingConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
        //   Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
        //   Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
        //   ICollectionMappingStrategy collectionStrategy,
        //   Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
        //   where TTargetProperty : IEnumerable
        //{
        //    var sourceMemberInfo = sourcePropertySelector.ExtractMemberInfo();
        //    var targetMemberInfo = targetPropertySelector.ExtractMemberInfo();

        //    var propertyMapping = base.Map( sourceMemberInfo, targetMemberInfo, converter );
        //    propertyMapping.TargetProperty.CollectionStrategy = collectionStrategy;

        //    return this;
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Configuration
{
    public class TypeMappingConfigurator
    {
        //Each source and target property can be instantiated only once per configuration
        //so we can handle their options correctly.
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

        public TypeMappingConfigurator MapMember( LambdaExpression sourceSelector,
            LambdaExpression targetSelector, LambdaExpression converter = null )
        {
            var targetMember = targetSelector.ExtractMember();

            var sourcePropConfig = this.GetOrAddMappingSource( sourceSelector );
            var targetPropConfig = this.GetOrAddTargetProperty( targetSelector, targetMember.GetSetterLambdaExpression() );

            var propertyMapping = new MemberMapping( _typeMapping, sourcePropConfig, targetPropConfig )
            {
                MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION,
                CustomConverter = converter
            };

            if( _typeMapping.MemberMappings.ContainsKey( targetMember ) )
                _typeMapping.MemberMappings[ targetMember ] = propertyMapping;
            else
                _typeMapping.MemberMappings.Add( targetMember, propertyMapping );

            return this;
        }

        protected MappingSource GetOrAddMappingSource( LambdaExpression memberSelector )
        {
            MemberInfo memberInfo = memberSelector.ExtractMember();

            MappingSource sourceProperty;
            if( !_sourceProperties.TryGetValue( memberInfo, out sourceProperty ) )
            {
                sourceProperty = new MappingSource( memberSelector );
                _sourceProperties.Add( memberInfo, sourceProperty );
            }

            return sourceProperty;
        }

        protected MappingTarget GetOrAddTargetProperty(
            LambdaExpression memberGetter, LambdaExpression memberSetter )
        {
            MemberInfo memberInfo = memberGetter.ExtractMember();

            MappingTarget targetProperty;
            if( !_targetProperties.TryGetValue( memberInfo, out targetProperty ) )
            {
                targetProperty = new MappingTarget( memberGetter, memberSetter );
                _targetProperties.Add( memberInfo, targetProperty );
            }

            return targetProperty;
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

            foreach( var sourceMember in sourceProperties.Cast<MemberInfo>().Concat( sourceFields ) )
            {
                foreach( var targetMember in targetProperties.Cast<MemberInfo>().Concat( targetFields ) )
                {
                    if( _globalConfiguration.MappingConvention.IsMatch( sourceMember, targetMember ) )
                    {
                        var targetMemberGetExpression = targetMember.GetGetterLambdaExpression();
                        var targetMemberSetExpression = targetMember.GetSetterLambdaExpression();

                        var sourcePropertyConfig = this.GetOrAddMappingSource(
                            sourceMember.GetGetterLambdaExpression() );

                        var targetPropertyConfig = this.GetOrAddTargetProperty(
                            targetMemberGetExpression, targetMemberSetExpression );

                        var propertyMapping = new MemberMapping( typeMapping, sourcePropertyConfig, targetPropertyConfig )
                        {
                            MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION
                        };

                        if( !typeMapping.MemberMappings.ContainsKey( targetMember ) )
                            typeMapping.MemberMappings.Add( targetMember, propertyMapping );
                        else
                            typeMapping.MemberMappings[ targetMember ] = propertyMapping;

                        break; //sourceProperty is now mapped, jump directly to the next sourceProperty
                    }
                }
            }
        }
    }

    public class TypeMappingConfigurator<TSource, TTarget> : TypeMappingConfigurator
    {
        public TypeMappingConfigurator( GlobalConfiguration globalConfiguration ) :
            base( typeof( TSource ), typeof( TTarget ), globalConfiguration )
        { }

        public TypeMappingConfigurator( TypeMapping typeMapping,
            GlobalConfiguration globalConfiguration ) : base( typeMapping, globalConfiguration )
        { }

        public TypeMappingConfigurator<TSource, TTarget> TargetConstructor(
            Expression<Func<TSource, TTarget>> constructor )
        {
            _typeMapping.CustomTargetConstructor = constructor;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> EnableMappingConventions()
        {
            _typeMapping.IgnoreConventions = false;
            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> IgnoreSourceProperty<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            params Expression<Func<TSource, TSourceProperty>>[] sourcePropertySelectors )
        {
            var selectors = new[] { sourcePropertySelector }
                .Concat( sourcePropertySelectors );

            foreach( var selector in selectors )
            {
                var sourceMember = this.GetOrAddMappingSource( selector );
                sourceMember.Ignore = true;
            }

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapMethod<TSourceProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Action<TTarget, TSourceProperty>> targetPropertySelector )
        {
            return (TypeMappingConfigurator<TSource, TTarget>)
                base.MapMember( sourcePropertySelector, targetPropertySelector, null );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourceSelector,
            Expression<Func<TTarget, TTargetProperty>> targetSelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
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

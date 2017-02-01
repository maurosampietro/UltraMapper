using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        protected MappingSource GetOrAddSourceProperty( MemberInfo MemberInfo )
        {
            MappingSource sourceProperty;
            if( !_sourceProperties.TryGetValue( MemberInfo, out sourceProperty ) )
            {
                sourceProperty = new MappingSource( MemberInfo );
                _sourceProperties.Add( MemberInfo, sourceProperty );
            }

            return sourceProperty;
        }

        protected MappingTarget GetOrAddTargetProperty( MemberInfo MemberInfo )
        {
            MappingTarget targetProperty;
            if( !_targetProperties.TryGetValue( MemberInfo, out targetProperty ) )
            {
                targetProperty = new MappingTarget( MemberInfo );
                _targetProperties.Add( MemberInfo, targetProperty );
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
                        var sourcePropertyConfig = this.GetOrAddSourceProperty( sourceMember );
                        var targetPropertyConfig = this.GetOrAddTargetProperty( targetMember );

                        var propertyMapping = new MemberMapping( typeMapping, sourcePropertyConfig, targetPropertyConfig )
                        {
                            MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION
                        };

                        propertyMapping.Mapper = _globalConfiguration.Mappers.FirstOrDefault(
                            mapper => mapper.CanHandle( propertyMapping ) );

                        if( propertyMapping.Mapper == null )
                            throw new Exception( $"No object mapper can handle {propertyMapping}" );

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
            var selectors = new[] { sourcePropertySelector }.Concat( sourcePropertySelectors );
            var properties = selectors.Select( prop => prop.ExtractMember() );

            foreach( var property in properties )
                this.GetOrAddSourceProperty( property ).Ignore = true;

            return this;
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
        {
            var sourceMemberInfo = sourcePropertySelector.ExtractMember();
            var targetMemberInfo = targetPropertySelector.ExtractMember();

            return this.MapProperty( sourceMemberInfo, targetMemberInfo, converter );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty( string sourcePropertyName,
            string targetPropertyName, LambdaExpression converter = null )
        {
            var sourceMemberInfo = _typeMapping.TypePair
                .SourceType.GetProperty( sourcePropertyName );

            var targetMemberInfo = _typeMapping.TypePair
                .TargetType.GetProperty( targetPropertyName );

            return this.MapProperty( sourceMemberInfo, targetMemberInfo, converter );
        }

        public TypeMappingConfigurator<TSource, TTarget> MapProperty( MemberInfo sourceProperty,
            MemberInfo targetProperty, LambdaExpression converter = null )
        {
            if( sourceProperty.ReflectedType != _typeMapping.TypePair.SourceType )
                throw new ArgumentException( $"'{sourceProperty}' does not belong to type '{_typeMapping.TypePair.SourceType}'" );

            if( targetProperty.ReflectedType != _typeMapping.TypePair.TargetType )
                throw new ArgumentException( $"'{targetProperty}' does not belong to type '{_typeMapping.TypePair.TargetType}'" );

            var sourcePropConfig = this.GetOrAddSourceProperty( sourceProperty );
            var targetPropConfig = this.GetOrAddTargetProperty( targetProperty );

            var propertyMapping = new MemberMapping( _typeMapping, sourcePropConfig, targetPropConfig )
            {
                MappingResolution = MappingResolution.RESOLVED_BY_CONVENTION,
                CustomConverter = converter
            };

            propertyMapping.Mapper = _globalConfiguration.Mappers.FirstOrDefault(
                mapper => mapper.CanHandle( propertyMapping ) );

            if( _typeMapping.MemberMappings.ContainsKey( targetProperty ) )
                _typeMapping.MemberMappings[ targetProperty ] = propertyMapping;
            else
                _typeMapping.MemberMappings.Add( targetProperty, propertyMapping );

            return this;
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

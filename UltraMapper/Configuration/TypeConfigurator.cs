using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.ExtensionMethods;
using UltraMapper.Internals;
using UltraMapper.Mappers;
using UltraMapper.MappingConventions;

namespace UltraMapper
{
    //public class TypeConfigurator
    //{
    //    public readonly GlobalConfiguration GlobalConfiguration;

    //    public TypeConfigurator()
    //    {
    //        GlobalConfiguration = new GlobalConfiguration( )
    //        {
    //            MappingConvention = new DefaultMappingConvention(),
    //        };
    //    }

    //    public TypeConfigurator( Action<TypeConfigurator> config )
    //        : this() { config?.Invoke( this ); }

    //    public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( Action<TypeMapping> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
    //    }

    //    /// <summary>
    //    /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    //    /// This overrides mapping conventions.
    //    /// </summary>
    //    /// <typeparam name="TSource">Source type</typeparam>
    //    /// <typeparam name="TTarget">Target type</typeparam>
    //    /// <param name="targetConstructor">The conversion mechanism to be used to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</param>
    //    /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
    //    public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
    //        Expression<Func<TSource, TTarget>> converter, Action<TypeMapping> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMapping.CustomConverter = converter;
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
    //    }

    //    /// <summary>
    //    /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    //    /// This overrides mapping conventions.
    //    /// </summary>
    //    /// <typeparam name="TSource">Source type</typeparam>
    //    /// <typeparam name="TTarget">Target type</typeparam>
    //    /// <param name="targetConstructor">The expression providing an instance of <typeparamref name="TTarget"/>.</param>
    //    /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
    //    public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>(
    //        Expression<Func<TTarget>> targetConstructor, Action<TypeMapping> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMapping.CustomTargetConstructor = targetConstructor;
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
    //    }

    //    /// <summary>
    //    /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    //    /// This overrides mapping conventions.
    //    /// </summary>
    //    /// <typeparam name="TSource">Source type</typeparam>
    //    /// <typeparam name="TTarget">Target type</typeparam>
    //    /// <param name="source">Source instance</param>
    //    /// <param name="target">Target instance</param>
    //    /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
    //    public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( TSource source, TTarget target,
    //        Action<TypeMapping> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( source.GetType(), target.GetType() );
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping, GlobalConfiguration );
    //    }

    //    private TypeMapping GetTypeMapping( Type source, Type target )
    //    {
    //        var typePair = new TypePair( source, target );

    //        return _typeMappings.GetOrAdd( typePair,
    //            () => new TypeMapping( GlobalConfiguration, typePair ) );
    //    }
    //}
}

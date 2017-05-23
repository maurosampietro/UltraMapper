using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Conventions;
using System.Collections;

namespace UltraMapper
{
    //public class TypeConfigurator
    //{
    //    public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget, TSourceElement, TTargetElement>(
    //      Expression<Func<TSourceElement, TTargetElement, bool>> elementEqualityComparison )
    //      where TSource : IEnumerable
    //      where TTarget : IEnumerable
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
    //        typeMapping.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
    //        typeMapping.CollectionMappingStrategy = CollectionMappingStrategies.UPDATE;
    //        typeMapping.CollectionItemEqualityComparer = elementEqualityComparison;

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping );
    //    }

    //    //public MemberConfigurator<IEnumerable<TSource>, IEnumerable<TTarget>> MapTypes<TSource, TTarget>(
    //    //    IEnumerable<TSource> source, IEnumerable<TTarget> target,
    //    //    Expression<Func<TSource, TTarget, bool>> elementEqualityComparison )
    //    //{
    //    //    var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //    //    typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
    //    //    typeMapping.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
    //    //    typeMapping.CollectionMappingStrategy = CollectionMappingStrategies.UPDATE;
    //    //    typeMapping.CollectionItemEqualityComparer = elementEqualityComparison;

    //    //    return new MemberConfigurator<IEnumerable<TSource>, IEnumerable<TTarget>>( typeMapping );
    //    //}

    //    public MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( Action<ITypeOptions> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping );
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
    //        Expression<Func<TSource, TTarget>> converter, Action<ITypeOptions> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
    //        typeMapping.CustomConverter = converter;
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping );
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
    //        Expression<Func<TTarget>> targetConstructor, Action<ITypeOptions> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( typeof( TSource ), typeof( TTarget ) );
    //        typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
    //        typeMapping.CustomTargetConstructor = targetConstructor;
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping );
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
    //        Action<ITypeOptions> typeMappingConfig = null )
    //    {
    //        var typeMapping = this.GetTypeMapping( source.GetType(), target.GetType() );
    //        typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
    //        typeMappingConfig?.Invoke( typeMapping );

    //        return new MemberConfigurator<TSource, TTarget>( typeMapping );
    //    }
    //}
}

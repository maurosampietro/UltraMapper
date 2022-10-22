using System;
using System.Collections;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper
{
    /// <summary>
    /// Type-to-Type mapping
    /// </summary>
    public static class TypeConfigurator
    {
        public static MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget, TSourceElement, TTargetElement>( this Configuration config,
            Expression<Func<TSourceElement, TTargetElement, bool>> elementEqualityComparison )
            where TSource : IEnumerable
            where TTarget : IEnumerable
        {
            var typeMapping = config[ typeof( TSource ), typeof( TTarget ) ];
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMapping.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
            typeMapping.CollectionBehavior = CollectionBehaviors.UPDATE;
            typeMapping.CollectionItemEqualityComparer = elementEqualityComparison;

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        public static MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( this Configuration config, Action<ITypeMappingOptions> typeMappingConfig = null )
        {
            var typeMapping = config[ typeof( TSource ), typeof( TTarget ) ];
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The conversion mechanism to be used to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public static MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( this Configuration config,
            Expression<Func<TSource, TTarget>> converter, Action<ITypeMappingOptions> typeMappingConfig = null )
        {
            var typeMapping = config[ typeof( TSource ), typeof( TTarget ) ];
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMapping.CustomConverter = converter;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The conversion mechanism to be used to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public static MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( this Configuration config,
            Expression<Func<ReferenceTracker, TSource, TTarget>> converter, Action<ITypeMappingOptions> typeMappingConfig = null )
        {
            var typeMapping = config[ typeof( TSource ), typeof( TTarget ) ]; 
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMapping.CustomConverter = converter;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="targetConstructor">The expression providing an instance of <typeparamref name="TTarget"/>.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public static MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( this Configuration config,
            Expression<Func<TTarget>> targetConstructor, Action<ITypeMappingOptions> typeMappingConfig = null )
        {
            var typeMapping = config[ typeof( TSource ), typeof( TTarget ) ];
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMapping.CustomTargetConstructor = targetConstructor;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TTarget">Target type</typeparam>
        /// <param name="source">Source instance</param>
        /// <param name="target">Target instance</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public static MemberConfigurator<TSource, TTarget> MapTypes<TSource, TTarget>( this Configuration config, TSource source, TTarget target,
            Action<ITypeMappingOptions> typeMappingConfig = null )
        {
            var typeMapping = config[ source.GetType(), target.GetType() ];
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator<TSource, TTarget>( typeMapping );
        }

        /// <summary>
        /// Lets you configure how to map from <paramref name="source"/> to <paramref name="target"/>.
        /// This overrides mapping conventions.
        /// </summary>
        /// <param name="source">Source instance</param>
        /// <param name="target">Target instance</param>
        /// <param name="typeMappingConfig">Allow you to configure the mapping you are mapping.</param>
        /// <returns>A strongly-typed member-mapping configurator for this type-mapping.</returns>
        public static MemberConfigurator MapTypes( this Configuration config, Type source, 
            Type target, Action<ITypeMappingOptions> typeMappingConfig = null )
        {
            var typeMapping = config[ source.GetType(), target.GetType() ];
            typeMapping.MappingResolution = MappingResolution.USER_DEFINED;
            typeMappingConfig?.Invoke( typeMapping );

            return new MemberConfigurator( typeMapping );
        }
    }
}

using System;
using UltraMapper.Internals;

namespace UltraMapper
{
    public sealed partial class Mapper
    {
        public readonly Configuration Config;       

        /// <summary>
        /// Initialize a new instance with the specified mapping configuration.
        /// </summary>
        /// <param name="config">The mapping configuration.</param>
        public Mapper( Configuration config )
        {
            this.Config = config;
        }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMatchingRuleEvaluator"/> 
        /// as mapping convention allowing inline editing of the configuraton itself.
        /// </summary>
        /// <param name="config"></param>
        public Mapper( Action<Configuration> config = null )
            : this( new Configuration() ) { config?.Invoke( this.Config ); }

        /// <summary>
        /// Maps <param name="source"/> on a new instance of the same type.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TSource Map<TSource>( TSource source ) where TSource : class, new()
        {
            if( source == null ) return null;

            var target = (TSource)InstanceFactory.CreateObject( source.GetType() );
            this.Map( source, target );

            return target;
        }

        /// <summary>
        /// Maps <param name="source"> on a new instance of type <typeparam name="TTarget">.
        /// </summary>
        /// <typeparam name="TTarget">Type of the new instance.</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TTarget Map<TTarget>( object source ) where TTarget : class, new()
        {
            if( source == null ) return null;

            var target = new TTarget();
            this.Map( source, target );
            return target;
        }

        /// <summary>
        /// Maps from <param name="source"/> to the existing instance <paramref name="target"/>
        /// Let's you reuse an existing <see cref="ReferenceTracker"/> cache.
        /// /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <typeparam name="TTarget">Type of the target instance.</typeparam>
        /// <param name="source">The source instance from which the values are read.</param>
        /// <param name="target">The target instance to which the values are written.</param>
        public void Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracker referenceTracking = null,
            ReferenceBehaviors refBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL )
            where TTarget : class
        {
            if( source == null )
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                target = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                return;
            }

            if( this.Config.IsReferenceTrackingEnabled )
            {
                Type targetType = target.GetType();

                if( referenceTracking == null )
                    referenceTracking = new ReferenceTracker();

                referenceTracking.Add( source, targetType, target );
            }

            //this.MappingConfiguration.ReferenceBehavior = refBehavior;

            var mapping = this.Config[ source.GetType(), target.GetType() ];
            //since we pass an existing target instance to map onto;
            //by default we use all of the existing instances we found on the target
            mapping.ReferenceBehavior = refBehavior;

            this.Map( source, target, referenceTracking, null );
        }

        //public class AbstractTypeMappingCrawler
        //{
        //    public IMapping GetMapping<TSource, TTarget>( TSource source, TTarget target, IMapping mapping )
        //    {
        //        Type mappingSourceType = null;
        //        Type mappingTargetType = null;

        //        if( mapping is TypeMapping typeMapping )
        //        {
        //            mappingSourceType = typeMapping.TypePair.SourceType;
        //            mappingTargetType = typeMapping.TypePair.TargetType;
        //        }
        //        else if( mapping is MemberMapping memberMapping &&
        //            memberMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
        //        {
        //            var memberTypeMapping = memberMapping.MemberTypeMapping;
        //            mappingSourceType = memberTypeMapping.TypePair.SourceType;
        //            mappingTargetType = memberTypeMapping.TypePair.TargetType;
        //        }

        //        if( (mappingSourceType.IsInterface || mappingSourceType.IsAbstract) &&
        //            (mappingTargetType.IsInterface || mappingTargetType.IsAbstract) )
        //        {
        //            return this.MappingConfiguration[ source.GetType(), target.GetType() ];
        //        }

        //        if( mappingSourceType.IsInterface || mappingSourceType.IsAbstract )
        //            return this.MappingConfiguration[ source.GetType(), mappingTargetType ];

        //        if( mappingTargetType.IsInterface || mappingTargetType.IsAbstract )
        //            return this.MappingConfiguration[ mappingSourceType, target.GetType() ];
        //    }
        //}

        internal void Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracker referenceTracking, IMapping mapping )
        {
            //in order to manage inheritance at runtime here
            //we check if a mapping has been defined and if it has not
            //we create a specific mapping at runtime.
            //A new mapping is created only if no compatible mapping is already available
            //for concrete classes. If a mapping for the interfaces is found, it is used.

            IMapping CheckResolveAbstractMapping( Type sourceType, Type targetType )
            {
                if( (sourceType.IsInterface || sourceType.IsAbstract) &&
                    (targetType.IsInterface || targetType.IsAbstract) )
                {
                    return this.Config[ source.GetType(), target.GetType() ];
                }

                if( sourceType.IsInterface || sourceType.IsAbstract )
                    return this.Config[ source.GetType(), targetType ];

                if( targetType.IsInterface || targetType.IsAbstract )
                    return this.Config[ sourceType, target.GetType() ];

                if( mapping == null )
                    return this.Config[ sourceType, targetType ];

                return mapping;
            }

            switch( mapping )
            {
                case MemberMapping memberMapping:
                {
                    if( memberMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION &&
                        memberMapping.InstanceTypeMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
                    {
                        var memberTypeMapping = memberMapping.MemberTypeMapping;

                        var mappingSourceType = memberTypeMapping.SourceType;
                        var mappingTargetType = memberTypeMapping.TargetType;

                        mapping = CheckResolveAbstractMapping( mappingSourceType, mappingTargetType );
                    }

                    break;
                }

                case TypeMapping typeMapping:
                {
                    var mappingSourceType = typeMapping.SourceType;
                    var mappingTargetType = typeMapping.TargetType;

                    mapping = CheckResolveAbstractMapping( mappingSourceType, mappingTargetType );
                    break;
                }

                case null:
                {
                    mapping = CheckResolveAbstractMapping( source.GetType(), target.GetType() );
                    break;
                }
            }

            mapping.MappingFunc.Invoke( referenceTracking, source, target );
        }
    }

    //type
    public sealed partial class Mapper
    {
        public object Map( object source, Type targetType )
        {
            if( source == null ) return null;

            var target = InstanceFactory.CreateObject( targetType );
            this.Map( source, target );

            return target;
        }
    }

    //structs
    public sealed partial class Mapper
    {
        public TTarget MapStruct<TSource, TTarget>( TSource source,
            ReferenceTracker referenceTracking = null ) where TTarget : struct
        {
            if( referenceTracking == null )
                referenceTracking = new ReferenceTracker();

            Type sourceType = typeof( TSource );
            Type targetType = typeof( TTarget );

            var mapping = this.Config[ sourceType, targetType ];

            if( sourceType.IsClass && !sourceType.IsBuiltIn( true ) )
            {
                var method = (Func<ReferenceTracker, TSource, TTarget, TTarget>)mapping.MappingExpression.Compile();
                return method.Invoke( referenceTracking, source, new TTarget() );
            }
            else
            {
                return (TTarget)mapping.MappingFuncPrimitives.Invoke( referenceTracking, source );
            }
        }

        public void Map<TSource, TTarget>( TSource source, out TTarget target,
            ReferenceTracker referenceTracking = null ) where TTarget : struct
        {
            if( referenceTracking == null )
                referenceTracking = new ReferenceTracker();

            Type sourceType = typeof( TSource );
            Type targetType = typeof( TTarget );

            var mapping = this.Config[ sourceType, targetType ];

            if( sourceType.IsClass && !sourceType.IsBuiltIn( true ) )
            {
                var method = (Func<ReferenceTracker, TSource, TTarget, TTarget>)mapping.MappingExpression.Compile();
                target = method.Invoke( referenceTracking, source, new TTarget() );
            }
            else
            {
                target = (TTarget)mapping.MappingFuncPrimitives.Invoke( referenceTracking, source );
            }
        }
    }

    //public partial class Mapper
    //{
    //    public object Map( object source, object target, ReferenceTracker referenceTracking = null )
    //    {
    //        var sourceType = source.GetType();
    //        var targetType = target.GetType();

    //        var mapping = this.MappingConfiguration[ sourceType, targetType ];
    //        mapping.MappingFunc( referenceTracking, source, target );

    //        return target;
    //    }
    //}
}
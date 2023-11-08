using System;
using System.Security.AccessControl;
using UltraMapper.Internals;

namespace UltraMapper
{
    public sealed partial class Mapper
    {
        public Configuration Config { get; init; }

        /// <summary>
        /// Initialize a new instance of the <see cref="Mapper"/> class with the specified configuration.
        /// </summary>
        /// <param name="config">The mapper configuration.</param>
        public Mapper( Configuration config )
        {
            this.Config = config ?? throw new ArgumentNullException( nameof( config ) );
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="Mapper"/> with the default configuration,
        /// allowing inline editing of the configuration itself.
        /// </summary>
        /// <param name="configSetup">An action that sets up configuration.</param>
        public Mapper( Action<Configuration> configSetup = null )
            : this( new Configuration() ) { configSetup?.Invoke( this.Config ); }

        /// <summary>
        /// Maps <param name="source"/> on a new instance of the same type.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <param name="source">The source instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TSource Map<TSource>( TSource source )
        {
            var target = (TSource)InstanceFactory.CreateObject( source.GetType() );

            return this.Map<TSource, TSource>( source, target, null );
        }

        /// <summary>
        /// Maps <param name="source"> on a new instance of type <typeparam name="TTarget">.
        /// </summary>
        /// <typeparam name="TTarget">Type of the new instance.</typeparam>
        /// <param name="source">The source instance to be copied.</param>
        /// <returns>A map of the source instance on a target instance of type <typeparamref name="TTarget"/> </returns>
        public TTarget MapArray<TTarget>( object source )
        {
            if( source == default ) return default;
            TTarget target = InstanceFactory.CreateObject<int, TTarget>( 0 );
            return this.Map( source, target );
        }

        /// <summary>
        /// Maps <param name="source"> on a new instance of type <typeparam name="TTarget">.
        /// </summary>
        /// <typeparam name="TTarget">Type of the new instance.</typeparam>
        /// <param name="source">The source instance to be copied.</param>
        /// <returns>A map of the source instance on a target instance of type <typeparamref name="TTarget"/> </returns>
        public TTarget Map<TTarget>( object source )
        {
            if( source == default ) return default;
            TTarget target = target = InstanceFactory.CreateObject<TTarget>();
            return this.Map( source, target );
        }

        /// <summary>
        /// Maps <param name="source"> on a new instance of type <typeparam name="TTarget">.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <typeparam name="TTarget">Type of the new instance.</typeparam>
        /// <param name="source">The source instance to be copied.</param>
        /// <returns>A map of the source instance on a new instance of type <typeparamref name="TTarget"/> </returns>
        public TTarget Map<TSource, TTarget>( TSource source )
        {
            /* This overload is useful if you want to pass as source a TSource derived instance
             * but really want to map TSource members only.
             */
            var target = InstanceFactory.CreateObject<TTarget>();
            return this.Map( source, target, null );
        }

        /// <summary>
        /// Maps from <param name="source"/> to the existing instance <paramref name="target"/>
        /// Let's you reuse an existing <see cref="ReferenceTracker"/> cache.
        /// /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <typeparam name="TTarget">Type of the target instance.</typeparam>
        /// <param name="source">The source instance from which the values are read.</param>
        /// <param name="target">The target instance to which the values are written.</param>
        public TTarget Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracker referenceTracking = null,
            ReferenceBehaviors refBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL )
        {
            Type sourceType = source?.GetType() ?? typeof( TSource );
            Type targetType = target?.GetType() ?? typeof( TTarget );

            if( this.Config.IsReferenceTrackingEnabled )
            {
                if( referenceTracking == null )
                    referenceTracking = new ReferenceTracker();

                referenceTracking.Add( source, targetType, target );
            }

            /*SINCE WE PASS AN EXISTING TARGET INSTANCE TO MAP ONTO
            *WE USE TO USE ALL OF THE EXISTING INSTANCES WE FOUND ON THE TARGET AS DEFAULT.
            *
            *WE CAN DECIDE WETHER TO APPLY THIS REFERENCE BEHAVIOR GLOBALLY OR 
            *ON THE SPECIFIC MAPPING CONFIGURATION OF THE TYPE INVOLVED
            */

            var mapping = this.Config[ sourceType, targetType ];
            mapping.ReferenceBehavior = refBehavior;

            return this.Map( source, target, referenceTracking, mapping );
        }

        internal TTarget Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracker referenceTracking, IMapping mapping )
        {
            //in order to manage inheritance at runtime here
            //we check if a mapping has been defined and if it has not
            //we create a specific mapping at runtime.
            //A new mapping is created only if no compatible mapping is already available
            //for concrete classes. If a mapping for the interfaces is found, it is used.

            //IMapping ResolveAbstractMapping( Type sourceType, Type targetType )
            //{
            //    if( (sourceType.IsInterface || sourceType.IsAbstract) &&
            //        (targetType.IsInterface || targetType.IsAbstract) )
            //    {
            //        return this.Config[ source.GetType(), target.GetType() ];
            //    }

            //    if( sourceType.IsInterface || sourceType.IsAbstract )
            //        return this.Config[ source.GetType(), targetType ];

            //    if( targetType.IsInterface || targetType.IsAbstract )
            //        return this.Config[ sourceType, target.GetType() ];

            //    if( mapping == null )
            //        return this.Config[ sourceType, targetType ];
            //    return mapping;
            //}

            ////removing abstract mapping resolution at this level entirely
            ////can save us 200ms (1100ms instead of 1300ms) on the current benchamark test.
            ////mapping resolution is a good but less used feature. can favor performance.
            //if( mapping == null )
            //{
            //    var targetType = target?.GetType() ?? typeof( TTarget );
            //    mapping = ResolveAbstractMapping( source.GetType(), targetType );
            //}
            //else if( mapping.NeedsRuntimeTypeInspection )
            //{
            //    switch( mapping )
            //    {
            //        case MemberMapping memberMapping:
            //        {
            //            if( memberMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION &&
            //                memberMapping.InstanceTypeMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
            //            {
            //                var memberTypeMapping = memberMapping.TypeToTypeMapping;

            //                var mappingSourceType = memberTypeMapping.Source.EntryType;
            //                var mappingTargetType = memberTypeMapping.Target.EntryType;

            //                mapping = ResolveAbstractMapping( mappingSourceType, mappingTargetType );
            //            }

            //            break;
            //        }

            //        case TypeMapping typeMapping:
            //        {
            //            var mappingSourceType = typeMapping.Source.EntryType;
            //            var mappingTargetType = typeMapping.Target.EntryType;

            //            mapping = ResolveAbstractMapping( mappingSourceType, mappingTargetType );
            //            break;
            //        }
            //    }
            //}

            return (TTarget)mapping.MappingFunc( referenceTracking, source, target );
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

            var targetInstance = new TTarget();
            return (TTarget)mapping.MappingFunc( referenceTracking, source, targetInstance );
        }

        public void Map<TSource, TTarget>( TSource source, out TTarget target,
            ReferenceTracker referenceTracking = null ) where TTarget : struct
        {
            if( referenceTracking == null )
                referenceTracking = new ReferenceTracker();

            Type sourceType = typeof( TSource );
            Type targetType = typeof( TTarget );

            var mapping = this.Config[ sourceType, targetType ];
            target = new TTarget();
            target = (TTarget)mapping.MappingFunc( referenceTracking, source, target );
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
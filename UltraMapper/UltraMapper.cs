using System;
using UltraMapper.Internals;

namespace UltraMapper
{
    public class Mapper
    {
        public Configuration MappingConfiguration { get; protected set; }

        /// <summary>
        /// Initialize a new instance with the specified mapping configuration.
        /// </summary>
        /// <param name="config">The mapping configuration.</param>
        public Mapper( Configuration config )
        {
            this.MappingConfiguration = config;
        }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMatchingRuleEvaluator"/> 
        /// as mapping convention allowing inline editing of the configuraton itself.
        /// </summary>
        /// <param name="config"></param>
        public Mapper( Action<Configuration> config = null )
            : this( new Configuration() ) { config?.Invoke( this.MappingConfiguration ); }

        /// <summary>
        /// Maps <param name="source"/> on a new instance of type <typeparam name="TTarget">.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <typeparam name="TTarget">Type of the new instance</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TTarget Map<TSource, TTarget>( TSource source ) where TTarget : class, new()
        {
            if( source == null ) return null;

            if( typeof( TTarget ) == typeof( object ) )
            {
                object target = InstanceFactory.CreateObject( source.GetType() );
                this.Map( source, target );
                return (TTarget)target;
            }
            else
            {
                var target = new TTarget();
                this.Map( source, target );
                return target;
            }
        }

        /// <summary>
        /// Maps <param name="source"/> on a new instance of the same type.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TSource Map<TSource>( TSource source ) where TSource : class, new()
        {
            if( source == null ) return null;

            if( typeof( TSource ) == typeof( object ) )
            {
                object target = InstanceFactory.CreateObject( source.GetType() );
                this.Map( source, target );
                return (TSource)target;
            }
            else
            {
                var target = new TSource();
                this.Map( source, target );
                return target;
            }
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

            if( typeof( TTarget ) == typeof( object ) )
            {
                object target = InstanceFactory.CreateObject( source.GetType() );
                this.Map( source, target );
                return (TTarget)target;
            }
            else
            {
                var target = new TTarget();
                this.Map( source, target );
                return target;
            }
        }

        public TTarget MapStruct<TSource, TTarget>( TSource source,
          ReferenceTracker referenceTracking = null ) where TTarget : struct
        {
            if( referenceTracking == null )
                referenceTracking = new ReferenceTracker();

            Type sourceType = typeof( TSource );
            Type targetType = typeof( TTarget );

            var mapping = this.MappingConfiguration[ sourceType, targetType ];

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

            var mapping = this.MappingConfiguration[ sourceType, targetType ];

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
            ReferenceBehaviors refBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL,
            bool cantrack = true )
            where TTarget : class
        {
            if( source == null )
            {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                target = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                return;
            }

            if( referenceTracking == null && cantrack )
                referenceTracking = new ReferenceTracker();

            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            if( cantrack )
                referenceTracking.Add( source, targetType, target );

            var mapping = this.MappingConfiguration[ sourceType, targetType ];
            //since we pass an existing target instance to map onto;
            //by default we use all of the existing instances we found on the target
            mapping.ReferenceBehavior = refBehavior;

            this.Map( source, target, referenceTracking, mapping );
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

            //---runtime checks for abstract classes and interfaces.
            IMapping CheckResolveAbstractMapping( Type sourceType, Type targetType )
            {
                if( (sourceType.IsInterface || sourceType.IsAbstract) &&
                    (targetType.IsInterface || targetType.IsAbstract) )
                {
                    return this.MappingConfiguration[ source.GetType(), target.GetType() ];
                }

                if( sourceType.IsInterface || sourceType.IsAbstract )
                    return this.MappingConfiguration[ source.GetType(), targetType ];

                if( targetType.IsInterface || targetType.IsAbstract )
                    return this.MappingConfiguration[ sourceType, target.GetType() ];

                return mapping;
            }

            if( mapping is TypeMapping typeMapping )
            {
                var mappingSourceType = typeMapping.TypePair.SourceType;
                var mappingTargetType = typeMapping.TypePair.TargetType;

                mapping = CheckResolveAbstractMapping( mappingSourceType, mappingTargetType );
            }
            else if( mapping is MemberMapping memberMapping )
            {
                if( memberMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
                {
                    var memberTypeMapping = memberMapping.MemberTypeMapping;
                    var mappingSourceType = memberTypeMapping.TypePair.SourceType;
                    var mappingTargetType = memberTypeMapping.TypePair.TargetType;

                    mapping = CheckResolveAbstractMapping( mappingSourceType, mappingTargetType );
                }
            }
            else if( mapping is TypeMappingOptionsInheritanceTraversal ttrav )
            {
                var mappingSourceType = ttrav.TypeMapping.TypePair.SourceType;
                var mappingTargetType = ttrav.TypeMapping.TypePair.TargetType;

                mapping = CheckResolveAbstractMapping( mappingSourceType, mappingTargetType );
            }
            else if( mapping is MemberMappingOptionsInheritanceTraversal mtrav )
            {
                if( mtrav.MemberMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
                {
                    var memberTypeMapping = mtrav.MemberMapping.MemberTypeMapping;
                    var mappingSourceType = memberTypeMapping.TypePair.SourceType;
                    var mappingTargetType = memberTypeMapping.TypePair.TargetType;

                    mapping = CheckResolveAbstractMapping( mappingSourceType, mappingTargetType );
                }
            }
            else if( mapping == null )
            {
                mapping = CheckResolveAbstractMapping( typeof( TSource ), typeof( TTarget ) );
            }
            //---ends of runtime checks for abstract classes and interfaces

#if DEBUG
            try
            {
                mapping.MappingFunc.Invoke( referenceTracking, source, target );
            }
            catch( Exception ex )
            {
                throw;
            }
#else
            mapping.MappingFunc.Invoke( referenceTracking, source, target );
#endif
        }
    }
}
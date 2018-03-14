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

            var target = new TTarget();
            this.Map( source, target );
            return target;
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

            var target = new TSource();
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

        public void Map<TSource, TTarget>( TSource source, out TTarget target,
            ReferenceTracking referenceTracking = null ) where TTarget : struct
        {
            /*TEMPORARY IMPLEMENTATION*/
            if( referenceTracking == null )
                referenceTracking = new ReferenceTracking();

            //Non è il massimo: salta la funzione di map principale
            // e non tiene in cache le espressioni generate.
            Type sourceType = typeof( TSource );
            Type targetType = typeof( TTarget );

            var mapping = this.MappingConfiguration[ sourceType, targetType ];

            if( mapping.MappingExpression.Parameters[ 0 ].Type == typeof( ReferenceTracking ) )
            {
                var method = (Func<ReferenceTracking, TSource, TTarget, TTarget>)mapping.MappingExpression.Compile();
                target = method.Invoke( referenceTracking, source, new TTarget() );
            }
            else
            {
                var method = (Func<TSource, TTarget>)mapping.MappingExpression.Compile();
                target = method.Invoke( source );
            }
        }

        /// <summary>
        /// Maps from <param name="source"/> to the existing instance <paramref name="target"/>
        /// Let's you reuse an existing <see cref="ReferenceTracking"/> cache.
        /// /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <typeparam name="TTarget">Type of the target instance.</typeparam>
        /// <param name="source">The source instance from which the values are read.</param>
        /// <param name="target">The target instance to which the values are written.</param>
        public void Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracking referenceTracking = null ) where TTarget : class
        {
            if( source == null )
            {
                target = null;
                return;
            }

            if( referenceTracking == null )
                referenceTracking = new ReferenceTracking();

            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            referenceTracking.Add( source, targetType, target );

            var mapping = this.MappingConfiguration[ sourceType, targetType ];
            this.Map( source, target, referenceTracking, mapping );
        }

        internal void Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracking referenceTracking, IMapping mapping )
        {
            //---runtime checks for abstract class/interface.

            //in order to manage inheritance at runtime here
            //we check if a mapping has been defined and if it has not
            //we create a specific mapping at runtime.
            //A new mapping is created only if no compatible mapping is already available
            //for concrete classes. If a mapping for the interfaces is found, it is used.

            //INTERFACES/ABSTRACT CLASSES AND THEIR CONFIG INHERITANCE HAVE TO BE CODED MORE CLEARLY

            var sourceType = source.GetType();
            var targetType = target.GetType();

            Type mappingSourceType;
            Type mappingTargetType;
            TypeMapping typeMapping = null;

            if( mapping is TypeMapping )
            {
                typeMapping = ((TypeMapping)mapping);
            }
            else if( mapping is MemberMapping )
            {
                typeMapping = ((MemberMapping)mapping).MemberTypeMapping;
            }

            mappingSourceType = typeMapping.TypePair.SourceType;
            mappingTargetType = typeMapping.TypePair.TargetType;

            if( mappingSourceType.IsInterface || mappingSourceType.IsAbstract )
            {
                IMappingOptions options = (IMappingOptions)mapping;

                if( !this.MappingConfiguration.Contains( source.GetType(), target.GetType() ) )
                {
                    //if mapped at runtime, copy options 
                    var newTypeMapping = this.MappingConfiguration[ source.GetType(), target.GetType() ];
                    newTypeMapping.CollectionBehavior = options.CollectionBehavior;
                    newTypeMapping.CollectionItemEqualityComparer = options.CollectionItemEqualityComparer;
                    newTypeMapping.ReferenceBehavior = options.ReferenceBehavior;
                    //not sure about copying this option
                    newTypeMapping.CustomTargetConstructor = options.CustomTargetConstructor;

                    mapping = newTypeMapping;
                }
                else
                {
                    //user defined or already mapped at runtime
                    mapping = this.MappingConfiguration[ source.GetType(), target.GetType() ];
                }
            }
            //-----ends of runtime checks for abstract class/interface

            mapping.MappingFunc.Invoke( referenceTracking, source, target );
        }
    }
}

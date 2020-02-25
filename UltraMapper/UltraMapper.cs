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
            ReferenceTracking referenceTracking = null, 
            ReferenceBehaviors refBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL ) 
            where TTarget : class
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
            //since we pass an existing target instance to map onto;
            //by default we use all of the existing instances we found on the target
            mapping.ReferenceBehavior = refBehavior;

            this.Map( source, target, referenceTracking, mapping );
        }   

        internal void Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracking referenceTracking, IMapping mapping )
        {
            //in order to manage inheritance at runtime here
            //we check if a mapping has been defined and if it has not
            //we create a specific mapping at runtime.
            //A new mapping is created only if no compatible mapping is already available
            //for concrete classes. If a mapping for the interfaces is found, it is used.

            //---runtime checks for abstract classes and interfaces.

            IMapping CheckResolveAbstractMapping( TypePair typePair )
            {
                Type sourceType = typePair.SourceType;
                Type targetType = typePair.TargetType;

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
            };

            if( mapping is TypeMapping typeMapping )
            {
                mapping = CheckResolveAbstractMapping( typeMapping.TypePair );
            }
            else if( mapping is MemberMapping memberMapping )
            {
                if( memberMapping.MappingResolution == MappingResolution.RESOLVED_BY_CONVENTION )
                    mapping = CheckResolveAbstractMapping( memberMapping.MemberTypeMapping.TypePair );
            }
            //---ends of runtime checks for abstract classes and interfaces

            mapping.MappingFunc.Invoke( referenceTracking, source, target );
        }
        
           /// <summary>
        /// Primitive thingy mapping arrays of values to a strong typed object.
        /// Useful for csv readers, parsers and stuff like that.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="values">arrays of values</param>
        /// <returns>An instance of <typeparamref name="T"/> populated with values</returns>
        public T MapValues<T>( string[] values ) where T : new()
        {
            var t = new T();
            var members = new TargetMemberProvider()
            {
                IgnoreMethods = true
            }.GetMembers( typeof( T ) );

            var sourceLambdaArg = Expression.Parameter( typeof( string[] ), "sourceInstance" );
            var targetLambdaArg = Expression.Parameter( typeof( T ), "targetInstance" );

            var expressions = new List<Expression>();
            int i = 0;
            foreach( var member in members )
            {
                var getter = member.GetSetterLambdaExpression();
                var memberAccessPath = new MemberAccessPath();
                memberAccessPath.Add( member );

                var memberMap = this.MappingConfiguration[ typeof( string ), member.GetMemberType() ];
                var arrayItemAccess = Expression.ArrayIndex( sourceLambdaArg, Expression.Constant( i, typeof( int ) ) );
                var conversion = memberMap.MappingExpression.Body.ReplaceParameter( arrayItemAccess, "sourceInstance" );

                var exp = memberAccessPath.GetSetterLambdaExpression().Body
                    .ReplaceParameter( targetLambdaArg, "instance" )
                    .ReplaceParameter( conversion, "value" );

                expressions.Add( exp );
                i++;
            }

            var lambda = Expression.Lambda<Action<string[], T>>( Expression.Block( expressions ), new[] { sourceLambdaArg, targetLambdaArg } );
            lambda.Compile()( values, t );

            return t;
        }
    }
}

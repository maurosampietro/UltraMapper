using System;
using System.Diagnostics;
using UltraMapper.Internals;
using UltraMapper.MappingConventions;

namespace UltraMapper
{
    //public class UltraMapper<T> : UltraMapper where T : IMappingConvention, new()
    //{
    //    public UltraMapper( Action<TypeConfigurator> config )
    //          : base( new TypeConfigurator() )
    //    {
    //        config?.Invoke( base.MappingConfiguration );
    //    }
    //}

    public class UltraMapper
    {
        public Configuration MappingConfiguration { get; protected set; }

        /// <summary>
        /// Initialize a new instance with the specified mapping configuration.
        /// </summary>
        /// <param name="config">The mapping configuration.</param>
        public UltraMapper( Configuration config )
        {
            this.MappingConfiguration = config;
        }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMappingConvention"/> 
        /// as mapping convention allowing inline editing of the configuraton itself.
        /// </summary>
        /// <param name="config"></param>
        public UltraMapper( Action<Configuration> config = null )
            : this( new Configuration() ) { config?.Invoke( this.MappingConfiguration ); }

        public TTarget Map<TSource, TTarget>( TSource source ) where TTarget : class, new()
        {
            var target = new TTarget();
            this.Map( source, target );
            return target;
        }

        /// <summary>
        /// Maps <paramref name="source"/> on a new instance of the same type.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TSource Map<TSource>( TSource source ) where TSource : class, new()
        {
            var target = new TSource();
            this.Map( source, target );
            return target;
        }

        /// <summary>
        /// Maps <paramref name="source"/> on a new instance of the same type.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TTarget Map<TTarget>( object source ) where TTarget : class, new()
        {
            var target = new TTarget();
            this.Map( source, target );
            return target;
        }

        public void Map<TSource, TTarget>( TSource source, out TTarget target )
            where TSource : struct
            where TTarget : struct
        {
            //Non è il massimo: salta la funzione di map principale
            // e non tiene in cache le espressioni generate.

            Type sourceType = typeof( TSource );
            Type targetType = typeof( TTarget );

            var mapping = this.MappingConfiguration[ sourceType, targetType ];
            var method = (Func<TSource, TTarget>)mapping.MappingExpression.Compile();

            target = method.Invoke( source );
        }

        /// <summary>
        /// Read the values from <paramref name="source"/> and writes them to <paramref name="target"/>
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <typeparam name="TTarget">Type of the target instance.</typeparam>
        /// <param name="source">The source instance from which the values are read.</param>
        /// <param name="target">The target instance to which the values are written.</param>
        public void Map<TSource, TTarget>( TSource source, TTarget target )
            where TTarget : class
        {
            var referenceTracking = new ReferenceTracking();
            referenceTracking.Add( source, target.GetType(), target );

            this.Map( source, target, referenceTracking );
        }

        public void Map<TSource, TTarget>( TSource source,
            TTarget target, ReferenceTracking referenceTracking )
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            var mapping = this.MappingConfiguration[ sourceType, targetType ];
            this.Map( source, target, referenceTracking, mapping );
        }

        internal void Map<TSource, TTarget>( TSource source, TTarget target,
            ReferenceTracking referenceTracking, IMapping mapping )
        {
            var references = mapping.MappingFunc?.Invoke( referenceTracking, source, target );
            if( references != null )
            {
                foreach( var reference in references )
                {
                    if( reference != null )
                        this.Map( reference.Source, reference.Target, referenceTracking );
                }
            }
        }
    }
}

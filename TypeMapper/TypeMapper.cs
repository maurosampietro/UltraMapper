using System;
using System.Diagnostics;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class TypeMapper<T> : TypeMapper where T : IMappingConvention, new()
    {
        public TypeMapper( Action<MapperConfiguration<T>> config )
              : base( new MapperConfiguration<T>() )
        {
            config?.Invoke( (MapperConfiguration<T>)base.MappingConfiguration );
        }
    }

    public class TypeMapper
    {
        public MapperConfiguration MappingConfiguration { get; protected set; }

        /// <summary>
        /// Initialize a new instance with the specified mapping configuration.
        /// </summary>
        /// <param name="config">The mapping configuration.</param>
        public TypeMapper( MapperConfiguration config )
        {
            this.MappingConfiguration = config;
        }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMappingConvention"/> 
        /// as mapping convention allowing inline editing of the configuraton itself.
        /// </summary>
        /// <param name="config"></param>
        public TypeMapper( Action<MapperConfiguration<DefaultMappingConvention>> config = null )
        {
            this.MappingConfiguration = new MapperConfiguration<DefaultMappingConvention>();
            config?.Invoke( (MapperConfiguration<DefaultMappingConvention>)this.MappingConfiguration );
        }

        /// <summary>
        /// Creates a copy of the source instance.
        /// </summary>
        /// <typeparam name="TSource">Type of the source instance.</typeparam>
        /// <param name="source">The instance to be copied.</param>
        /// <returns>A deep copy of the source instance.</returns>
        public TSource Map<TSource>( TSource source ) where TSource : new()
        {
            var target = new TSource();
            this.Map( source, target );
            return target;
        }

        public void Map<TSource, TTarget>( TSource source, ref TTarget target )
            where TSource : struct
            where TTarget : struct
        {
            //Non è il massimo: salta la funzione di map principale
            // e non tiene in cache le espressioni generate.

            Type sourceType = source.GetType();
            Type targetType = target.GetType();

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
        /// <param name="target">the target instance to which the values are written.</param>
        public void Map<TSource, TTarget>( TSource source, TTarget target )
        {
            var referenceTracking = new ReferenceTracking();
            referenceTracking.Add( source, target.GetType(), target );

            this.Map( source, target, referenceTracking );
        }

        private void Map<TSource, TTarget>( TSource source,
            TTarget target, ReferenceTracking referenceTracking )
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            var mapping = this.MappingConfiguration[ sourceType, targetType ];

            var references = mapping.MapperFunc?.Invoke( referenceTracking, source, target );
            if( references != null )
            {
                foreach( var reference in references )
                    this.Map( reference.Source, reference.Target, referenceTracking );
            }
        }
    }
}

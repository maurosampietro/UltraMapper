using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class TypeMapper<T> : TypeMapper where T : IMappingConvention, new()
    {
        public TypeMapper( Action<MappingConfiguration<T>> config )
              : base( new MappingConfiguration<T>() )
        {
            config?.Invoke( (MappingConfiguration<T>)_mappingConfiguration );
        }
    }

    public class TypeMapper
    {
        public MappingConfiguration _mappingConfiguration { get; protected set; }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMappingConvention"/> 
        /// as mapping convention
        /// </summary>
        public TypeMapper() : this( new MappingConfiguration() ) { }

        /// <summary>
        /// Initialize a new instance with the specified mapping configuration.
        /// </summary>
        /// <param name="config">The mapping configuration.</param>
        public TypeMapper( MappingConfiguration config )
        {
            _mappingConfiguration = config;
        }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMappingConvention"/> 
        /// as mapping convention allowing inline editing of the configuraton itself.
        /// </summary>
        /// <param name="config"></param>
        public TypeMapper( Action<DefaultMappingConvention> config )
            : this( new MappingConfiguration( config ) ) { }

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

            var mappings = _mappingConfiguration[ sourceType, targetType ];

            var references = mappings.MapperFunc?.Invoke( referenceTracking, source, target );
            if( references != null )
            {
                foreach( var reference in references )
                    this.Map( reference.Source, reference.Target, referenceTracking );
            }
        }
    }
}

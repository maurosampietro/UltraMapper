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
using TypeMapper.MappingConventions;

namespace TypeMapper
{
    public class TypeMapper<T> : TypeMapper where T : IMappingConvention, new()
    {
        public TypeMapper( Action<TypeConfiguration<T>> config )
              : base( new TypeConfiguration<T>() )
        {
            config?.Invoke( (TypeConfiguration<T>)_mappingConfiguration );
        }
    }

    public class TypeMapper
    {
        protected TypeConfiguration _mappingConfiguration;

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMappingConvention"/> 
        /// as mapping convention
        /// </summary>
        public TypeMapper() : this( new TypeConfiguration() ) { }

        /// <summary>
        /// Initialize a new instance with the specified mapping configuration.
        /// </summary>
        /// <param name="config">The mapping configuration.</param>
        public TypeMapper( TypeConfiguration config )
        {
            _mappingConfiguration = config;
        }

        /// <summary>
        /// Initialize a new instance using <see cref="DefaultMappingConvention"/> 
        /// as mapping convention allowing inline editing of the configuraton itself.
        /// </summary>
        /// <param name="config"></param>
        public TypeMapper( Action<DefaultMappingConvention> config )
            : this( new TypeConfiguration( config ) ) { }

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
            this.Map( source, target, referenceTracking );
        }

        private void Map<TSource, TTarget>( TSource source,
            TTarget target, IReferenceTracking referenceTracking )
        {
            Type sourceType = source.GetType();
            Type targetType = target.GetType();

            var typeMappings = _mappingConfiguration[ sourceType, targetType ];
            var propertyMappings = typeMappings.GetPropertyMappings();

            foreach( var mapping in propertyMappings )
            {
                object value = mapping.SourceProperty.ValueGetter( source );
                if( mapping.ValueConverter != null )
                    value = mapping.ValueConverter( value );

                var targetValues = mapping.Mapper.Map( value,
                    target, mapping, referenceTracking );

                foreach( var refValue in targetValues )
                    this.Map( refValue.Source, refValue.Target, referenceTracking );
            }
        }
    }
}

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

    /// <summary>
    /// Le proprietà con lo stesso nome e dello stesso tipo vengono copiate nell'oggetto destinazione
    /// </summary>
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

            var typeMapper = _mappingConfiguration[ sourceType, targetType ];
            var propertyMappings = typeMapper.GetPropertyMappings();

            foreach( var mapping in propertyMappings )
            {
                var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
                var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

                object sourcePropertyValue = mapping.SourceProperty.ValueGetter( source );
                if( mapping.ValueConverter != null )
                    sourcePropertyValue = mapping.ValueConverter( sourcePropertyValue );

                if( mapping.SourceProperty.IsBuiltInType )
                {
                    if( sourcePropertyType != targetPropertyType )
                    {
                        //Convert.ChangeType does not handle conversion to nullable types
                        var conversionType = targetPropertyType;
                        if( mapping.TargetProperty.NullableUnderlyingType != null )
                            conversionType = mapping.TargetProperty.NullableUnderlyingType;

                        try
                        {
                            if( sourcePropertyValue == null && conversionType.IsValueType )
                                sourcePropertyValue = conversionType.GetDefaultValue();
                            else
                                sourcePropertyValue = Convert.ChangeType( sourcePropertyValue, conversionType );
                        }
                        catch( Exception ex )
                        {
                            // TODO: display generic arguments instead (for example: Nullable<int> instead of Nullable'1)

                            string errorMsg = $"Cannot automatically convert value from '{sourcePropertyType.Name}' to '{targetPropertyType.Name}'. " +
                                $"Please provide a converter for mapping '{mapping.SourceProperty.PropertyInfo.Name} -> {mapping.TargetProperty.PropertyInfo.Name}'";

                            throw new Exception( errorMsg, ex );
                        }
                    }

                    mapping.TargetProperty.ValueSetter( target, sourcePropertyValue );
                }
                //Collection: IsEnumerable and !BuiltInType (to avoid string type)
                else if( mapping.SourceProperty.IsEnumerable )
                {
                    var collection = (IList)Activator.CreateInstance( targetPropertyType );

                    Type genericType = targetPropertyType.GetCollectionGenericType();
                    bool isBuiltInType = genericType.IsBuiltInType( false );

                    foreach( var sourceItem in (IEnumerable)sourcePropertyValue )
                    {
                        object targetItem;
                        if( isBuiltInType )
                        {
                            targetItem = sourceItem;
                        }
                        else
                        {
                            if( !referenceTracking.TryGetValue( sourceItem,
                                genericType, out targetItem ) )
                            {
                                targetItem = Activator.CreateInstance( genericType );
                                
                                //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                                referenceTracking.Add( sourceItem, genericType, targetItem );
                                this.Map( sourceItem, targetItem, referenceTracking );
                            }
                        }

                        collection.Add( targetItem );
                    }

                    mapping.TargetProperty.ValueSetter( target, collection );
                }
                else
                {
                    if( sourcePropertyValue == null )
                        mapping.TargetProperty.ValueSetter( target, null );
                    else
                    {
                        object targetPropertyValue = null;
                        if( !referenceTracking.TryGetValue( sourcePropertyValue,
                            targetPropertyType, out targetPropertyValue ) )
                        {
                            targetPropertyValue = InstanceFactory.CreateObject( targetPropertyType );

                            //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                            referenceTracking.Add( sourcePropertyValue, targetPropertyType, targetPropertyValue );
                            this.Map( sourcePropertyValue, targetPropertyValue, referenceTracking );
                        }

                        mapping.TargetProperty.ValueSetter( target, targetPropertyValue );
                    }
                }
            }
        }
    }
}

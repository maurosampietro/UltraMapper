using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    /// <summary>
    /// Le proprietà con lo stesso nome e dello stesso tipo vengono copiate nell'oggetto destinazione
    /// </summary>
    public class TypeMapper
    {
        private TypeConfiguration _mappingConfiguration = new TypeConfiguration();

        public TypeMapper() : this( new TypeConfiguration() ) { }

        public TypeMapper( TypeConfiguration config )
        {
            _mappingConfiguration = config;
        }

        public TSource Map<TSource>( TSource source ) where TSource : new()
        {
            var destination = new TSource();
            this.Map( source, destination );
            return destination;
        }

        public void Map<TSource, TDestination>( TSource source, TDestination destination )
        {
            var referenceTracking = new ReferenceTracking();
            this.Map( source, destination, referenceTracking );
        }

        //public void Map( object source, object destination )
        //{
        //    Type sourceType = source.GetType();
        //    Type destinationType = destination.GetType();

        //    var typeMapper = _mappingConfiguration[ sourceType, destinationType ];
        //    if( typeMapper == null ) typeMapper = _mappingConfiguration.Map( sourceType, destinationType );

        //    var propertyMappings = typeMapper.GetPropertyMappings();
        //    this.Map( source, destination, propertyMappings, new HashSet<object>() );
        //}

        private void Map<TSource, TDestination>( TSource source,
            TDestination destination, IReferenceTracking referenceTracking )
        {
            Type sourceType = source.GetType();
            Type destinationType = destination.GetType();

            var typeMapper = _mappingConfiguration[ sourceType, destinationType ];
            if( typeMapper == null ) typeMapper = _mappingConfiguration.Map( sourceType, destinationType );

            var propertyMappings = typeMapper.GetPropertyMappings();
            foreach( var mapping in propertyMappings )
            {
                var sourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
                var destinationPropertyType = mapping.DestinationProperty.PropertyInfo.PropertyType;

                object sourcePropertyValue = mapping.SourceProperty.ValueGetter( source );
                if( mapping.ValueConverter != null )
                    sourcePropertyValue = mapping.ValueConverter( sourcePropertyValue );

                if( mapping.SourceProperty.IsBuiltInType )
                {
                    if( sourcePropertyType != destinationPropertyType )
                    {
                        //Convert.ChangeType does not handle conversion to nullable types
                        var conversionType = destinationPropertyType;
                        if( mapping.DestinationProperty.NullableUnderlyingType != null )
                            conversionType = mapping.DestinationProperty.NullableUnderlyingType;

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

                            string errorMsg = $"Cannot automatically convert value from '{sourcePropertyType.Name}' to '{destinationPropertyType.Name}'. " +
                                $"Please provide a converter for mapping '{mapping.SourceProperty.PropertyInfo.Name} -> {mapping.DestinationProperty.PropertyInfo.Name}'";

                            throw new Exception( errorMsg, ex );
                        }
                    }

                    mapping.DestinationProperty.ValueSetter( destination, sourcePropertyValue );
                }
                else
                {
                    if( sourcePropertyValue == null )
                        mapping.DestinationProperty.ValueSetter( destination, null );
                    else
                    {
                        object destinationPropertyValue = null;
                        if( !referenceTracking.TryGetValue( sourcePropertyValue,
                            destinationPropertyType, out destinationPropertyValue ) )
                        {
                            destinationPropertyValue = Activator.CreateInstance( destinationPropertyType );

                            //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                            referenceTracking.Add( sourcePropertyValue, destinationPropertyType, destinationPropertyValue );

                            //Collection
                            if( mapping.SourceProperty.IsEnumerable )
                            {
                                var collection = (IList)destinationPropertyValue;

                                Type genericType = destinationPropertyType.GetGenericType();
                                bool isBuiltInType = genericType.IsBuiltInType( false );

                                foreach( var sourceItem in (IEnumerable)sourcePropertyValue )
                                {
                                    object destinationItem;
                                    if( isBuiltInType )
                                    {
                                        destinationItem = Activator.CreateInstance( genericType );
                                        destinationItem = sourceItem;
                                    }
                                    else
                                    {
                                        if( !referenceTracking.TryGetValue( sourceItem,
                                            genericType, out destinationItem ) )
                                        {
                                            destinationItem = Activator.CreateInstance( genericType );

                                            //track these references BEFORE recursion to avoid infinite loops and stackoverflow
                                            referenceTracking.Add( sourceItem, genericType, destinationItem );
                                            this.Map( sourceItem, destinationItem, referenceTracking );
                                        }
                                    }

                                    collection.Add( destinationItem );
                                }
                            }
                            else this.Map( sourcePropertyValue, destinationPropertyValue, referenceTracking );
                        }

                        mapping.DestinationProperty.ValueSetter( destination, destinationPropertyValue );
                    }
                }
            }
        }
    }


    public interface ICollectionMapper
    {
        void AddItem( IList collection, object item );
    }

    /// <summary>
    /// Creates a new collection and assign it to the input collection
    /// </summary>
    public class ReplaceCollection : ICollectionMapper
    {
        public void AddItem( IList collection, object item )
        {
            collection.Add( item );
        }
    }

    ///// <summary>
    ///// Keeps using the input collection and maps
    ///// removing and adding element to it.
    ///// </summary>
    //public class UpdateCollection : ICollectionMapper { }

    ///// <summary>
    ///// Keeps the input collection and adds elements to it
    ///// </summary>
    //public class MergeCollection : ICollectionMapper { }


    public class PropertyMapping : PropertyMapping<object, object>
    {
        public PropertyMapping( SourceProperty sourceProperty,
            DestinationProperty destinationProperty = null,
            Func<object, object> converter = null ) : base( sourceProperty )
        {

        }
    }

    public class PropertyMapping<TSource, TDestination>
    {
        public SourceProperty<TSource> SourceProperty { get; private set; }
        public DestinationProperty<TDestination> DestinationProperty { get; set; }

        //Generalize to Fun<object,object> to avoid carrying too many generic T types around
        //and using Delegate and DynamicInvoke.
        public Func<object, object> ValueConverter { get; set; }

        public PropertyMapping( SourceProperty<TSource> sourceProperty,
            DestinationProperty<TDestination> destinationProperty = null,
            Func<object, object> converter = null )
        {
            this.SourceProperty = sourceProperty;
            this.DestinationProperty = destinationProperty;
            this.ValueConverter = converter;
        }
    }

    public class BaseProperty
    {
        public readonly PropertyInfo PropertyInfo;

        public BaseProperty( PropertyInfo propertyInfo )
        {
            this.PropertyInfo = propertyInfo;
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as SourceProperty;
            if( typePair == null ) return false;

            return this.PropertyInfo.Equals( typePair.PropertyInfo );
        }

        public override int GetHashCode()
        {
            return this.PropertyInfo.GetHashCode();
        }
    }

    public class SourceProperty<TSource> : BaseProperty
    {
        //This info is evaluated at configuration level only once for performance reasons
        public bool IsBuiltInType { get; set; }
        public bool IsEnumerable { get; set; }
        public Func<TSource, object> ValueGetter { get; set; }

        public SourceProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }

    public class DestinationProperty<TDestination> : BaseProperty
    {
        //This info is evaluated at configuration level only once for performance reasons
        public Type NullableUnderlyingType { get; set; }
        public Action<TDestination, object> ValueSetter { get; set; }

        public DestinationProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }

    public class SourceProperty : SourceProperty<object>
    {
        public SourceProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }

    public class DestinationProperty : DestinationProperty<object>
    {
        public DestinationProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace TypeMapper.Configuration
{
    public class PropertyConfiguration : IEnumerable<PropertyMapping>
    {
        //A source property can be mapped to multiple target properties
        private Dictionary<PropertyInfoPair, PropertyMapping> _propertyMappings;
        private IEnumerable<IObjectMapperExpression> _objectMappers;

        private LambdaExpression _expression;
        public LambdaExpression MappingExpression
        {
            get
            {
                if( _expression != null ) return _expression;
                if( !_propertyMappings.Any() ) return null;

                var returnType = typeof( List<ObjectPair> );
                var returnElementType = typeof( ObjectPair );

                var sourceType = _propertyMappings.Values.First().SourceProperty.PropertyInfo.DeclaringType;
                var targetType = _propertyMappings.Values.First().TargetProperty.PropertyInfo.DeclaringType;
                var trackerType = typeof( ReferenceTracking );

                var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
                var targetInstance = Expression.Parameter( targetType, "targetInstance" );
                var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

                var newRefObjects = Expression.Variable( returnType, "result" );

                var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var addRangeMethod = returnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
                var addCalls = _propertyMappings.Values.Select( mapping =>
                {
                    if( mapping.Expression.ReturnType == typeof( IEnumerable<ObjectPair> ) )
                    {
                        return Expression.Call( newRefObjects, addRangeMethod, mapping.Expression.Body );
                    }
                    else if( mapping.Expression.ReturnType == returnElementType )
                    {
                        var objPair = Expression.Variable( returnElementType, "objPair" );

                        return (Expression)Expression.Block
                        (
                            new[] { objPair },

                            Expression.Assign( objPair, mapping.Expression.Body ),

                            Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
                                Expression.Call( newRefObjects, addMethod, objPair ) )

                        );
                    }
                    else if( mapping.Expression.ReturnType == typeof( void ) )
                    {
                        return mapping.Expression.Body;
                    }

                    throw new ArgumentException( "Expressions should return System.Void or ObjectPair or IEnumerable<ObjectPair>" );
                } );

                var bodyExp = (addCalls?.Any() != true) ?
                        (Expression)Expression.Empty() : Expression.Block( addCalls );

                var body = (Expression)Expression.Block
                (
                    new[] { newRefObjects },

                    Expression.Assign( newRefObjects, Expression.New( returnType ) ),

                    bodyExp.ReplaceParameter( referenceTrack )
                        .ReplaceParameter( targetInstance, "targetInstance" )
                        .ReplaceParameter( sourceInstance, "sourceInstance" ),

                    newRefObjects
                );

                var delegateType = typeof( Func<,,,> ).MakeGenericType(
                    trackerType, sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

                return _expression = Expression.Lambda( delegateType,
                    body, referenceTrack, sourceInstance, targetInstance );
            }
        }

        private Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> _mapperFunc;
        public Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> MapperFunc
        {
            get
            {
                if( _mapperFunc != null ) return _mapperFunc;
                if( !_propertyMappings.Any() ) return null;

                var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );
                var sourceLambdaArg = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetLambdaArg = Expression.Parameter( typeof( object ), "targetInstance" );

                var sourceType = _propertyMappings.Values.First().SourceProperty.PropertyInfo.DeclaringType;
                var targetType = _propertyMappings.Values.First().TargetProperty.PropertyInfo.DeclaringType;

                var sourceInstance = Expression.Convert( sourceLambdaArg, sourceType );
                var targetInstance = Expression.Convert( targetLambdaArg, targetType );

                var bodyExp = Expression.Invoke( this.MappingExpression,
                    referenceTrack, sourceInstance, targetInstance );

                return _mapperFunc = Expression.Lambda<Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>>>(
                    bodyExp, referenceTrack, sourceLambdaArg, targetLambdaArg ).Compile();
            }
        }

        /// <summary>
        /// This constructor is only used by derived classes to allow
        /// casts from PropertyConfiguration to the derived class itself
        /// </summary>
        /// <param name="configuration">An already existing mapping configuration</param>
        protected PropertyConfiguration( PropertyConfiguration configuration, IEnumerable<IObjectMapperExpression> objectMappers )
        {
            _propertyMappings = configuration._propertyMappings;
            _objectMappers = objectMappers;
        }

        public PropertyConfiguration( Type source, Type target, IEnumerable<IObjectMapperExpression> objectMappers )
            : this( source, target, new DefaultMappingConvention(), objectMappers ) { }

        public PropertyConfiguration( Type source, Type target,
            IMappingConvention mappingConvention, IEnumerable<IObjectMapperExpression> objectMappers )
        {
            if( objectMappers == null || !objectMappers.Any() )
                throw new ArgumentException( "Please provide at least one object mapper" );

            _objectMappers = objectMappers;
            _propertyMappings = new Dictionary<PropertyInfoPair, PropertyMapping>();

            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;

            var sourceProperties = source.GetProperties( bindingAttributes )
                .Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

            var targetProperties = target.GetProperties( bindingAttributes )
                .Where( p => p.CanWrite && p.GetIndexParameters().Length == 0 ); //no indexed properties

            foreach( var sourceProperty in sourceProperties )
            {
                foreach( var targetProperty in targetProperties )
                {
                    if( targetProperty.SetMethod != null )
                    {
                        if( mappingConvention.IsMatch( sourceProperty, targetProperty ) )
                        {
                            this.Map( sourceProperty, targetProperty );
                            break; //sourceProperty is now mapped, jump directly to the next sourceProperty
                        }
                    }
                }
            }
        }

        protected PropertyMapping Map( PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo )
        {
            var typePairKey = new PropertyInfoPair( sourcePropertyInfo, targetPropertyInfo );

            PropertyMapping propertyMapping;
            if( !_propertyMappings.TryGetValue( typePairKey, out propertyMapping ) )
            {
                var sourceProperty = new SourceProperty( sourcePropertyInfo );

                propertyMapping = new PropertyMapping( sourceProperty );
                _propertyMappings.Add( typePairKey, propertyMapping );
            }

            propertyMapping.TargetProperty = new TargetProperty( targetPropertyInfo );
            propertyMapping.Mapper = _objectMappers.FirstOrDefault(
                mapper => mapper.CanHandle( propertyMapping ) );

            if( propertyMapping.Mapper == null )
                throw new Exception( $"No object mapper can handle {propertyMapping}" );

            return propertyMapping;
        }

        internal PropertyMapping this[ PropertyInfo sourceProperty, PropertyInfo targetProperty ]
        {
            get
            {
                var typePairKey = new PropertyInfoPair( sourceProperty, targetProperty );
                return _propertyMappings[ typePairKey ];
            }
        }

        internal PropertyMapping this[ PropertyInfoPair typePairKey ]
        {
            get { return _propertyMappings[ typePairKey ]; }
        }

        public IEnumerator<PropertyMapping> GetEnumerator()
        {
            return _propertyMappings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class PropertyConfiguration<TSource, TTarget> : PropertyConfiguration
    {
        /// <summary>
        /// This constructor is only used internally to allow
        /// casts from PropertyConfiguration to PropertyConfiguration<>
        /// </summary>
        /// <param name="map">An already existing mapping configuration</param>
        internal PropertyConfiguration( PropertyConfiguration map, IEnumerable<IObjectMapperExpression> objectMappers )
            : base( map, objectMappers ) { }

        public PropertyConfiguration( IMappingConvention mappingConvention, IEnumerable<IObjectMapperExpression> objectMappers )
            : base( typeof( TSource ), typeof( TTarget ), mappingConvention, objectMappers ) { }


        #region Type mapping overloads

        /*DO NOT try to merge the following 2 methods in one using optional parameters:
         *you lose the hint to provide a converter if a conversion is not available if you do*/

        /// <summary>
        /// Maps two properties of the same or compatible type
        /// </summary>
        /// <typeparam name="TSourceProperty"></typeparam>
        /// <param name="sourcePropertySelector"></param>
        /// <param name="targetPropertySelector"></param>
        /// <returns></returns>
        //public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty>(
        //    Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
        //    Expression<Func<TTarget, TSourceProperty>> targetPropertySelector )
        //{
        //    var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
        //    var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

        //    var propertyMapping = base.Map( sourcePropertyInfo, targetPropertyInfo );
        //    return this;
        //}

        /// <summary>
        /// Maps two properties providing a converter
        /// </summary>
        /// <typeparam name="TSourceProperty"></typeparam>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="sourcePropertySelector"></param>
        /// <param name="targetPropertySelector"></param>
        /// <param name="converter"></param>
        /// <returns></returns>
        public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
            var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

            var propertyMapping = base.Map( sourcePropertyInfo, targetPropertyInfo );
            propertyMapping.CustomConverter = converter;

            return this;
        }
        #endregion

        #region Collection overloads
        public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
           Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
           Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
           ICollectionMappingStrategy collectionStrategy,
           Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
           where TTargetProperty : IEnumerable
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
            var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

            var propertyMapping = base.Map( sourcePropertyInfo, targetPropertyInfo );

            propertyMapping.CustomConverter = converter;
            propertyMapping.TargetProperty.CollectionStrategy = collectionStrategy;

            return this;
        }
        #endregion
    }
}

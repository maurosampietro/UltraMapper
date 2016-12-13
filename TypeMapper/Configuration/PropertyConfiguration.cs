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
                if( _expression != null )
                    return _expression;

                var returnType = typeof( List<ObjectPair> );

                var sourceType = _propertyMappings.Values.First().SourceProperty.PropertyInfo.DeclaringType;
                var targetType = _propertyMappings.Values.First().TargetProperty.PropertyInfo.DeclaringType;
                var trackerType = typeof( ReferenceTracking );

                var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
                var targetInstance = Expression.Parameter( targetType, "targetInstance" );
                var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

                var newRefObjects = Expression.Variable( returnType, "result" );

                var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var addCalls = _propertyMappings.Values.Select( mapping =>
                {
                    if( mapping.Mapper is BuiltInTypeMapper )
                    {
                        return (Expression)Expression.Invoke( mapping.Expression, referenceTrack, sourceInstance, targetInstance );
                    }
                    else
                    {

                        var objPairs = Expression.Variable( typeof( IEnumerable<ObjectPair> ), "objPairs" );
                        var objPair = Expression.Variable( typeof( ObjectPair ), "objPair" );
                        var loopVar = Expression.Parameter( typeof( ObjectPair ), "loopVar" );

                        var loopContent = Expression.IfThen( Expression.NotEqual( loopVar, Expression.Constant( null ) ),
                            Expression.Call( newRefObjects, addMethod, loopVar ) );

                        return (Expression)Expression.Block
                        (
                            new[] { objPairs },

                            Expression.Assign( objPairs, Expression.Invoke( mapping.Expression,
                                referenceTrack, sourceInstance, targetInstance ) ),

                            Expression.IfThen
                            (
                                Expression.NotEqual( objPairs, Expression.Constant( null ) ),
                                ForEach( objPairs, loopVar, loopContent )
                            )
                        );
                    }
                } );

                var bodyExp = (addCalls?.Any() != true) ?
                        (Expression)Expression.Empty() : Expression.Block( addCalls );

                var body = Expression.Block
                (
                    new[] { newRefObjects },

                    Expression.Assign( newRefObjects, Expression.New( returnType ) ),
                    bodyExp,
                    newRefObjects
                );

                var delegateType = typeof( Func<,,,> ).MakeGenericType(
                    trackerType, sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

                return _expression = Expression.Lambda( delegateType,
                    body, referenceTrack, sourceInstance, targetInstance );
            }
        }

        public static Expression ForEach( Expression collection, ParameterExpression loopVar,
            Expression loopContent, Expression outLoopInitializations = null )
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof( IEnumerable<> ).MakeGenericType( elementType );
            var enumeratorType = typeof( IEnumerator<> ).MakeGenericType( elementType );

            var enumeratorVar = Expression.Variable( enumeratorType, "enumerator" );
            var getEnumeratorCall = Expression.Call( collection, enumerableType.GetMethod( "GetEnumerator" ) );
            var enumeratorAssign = Expression.Assign( enumeratorVar, getEnumeratorCall );

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call( enumeratorVar, typeof( IEnumerator ).GetMethod( "MoveNext" ) );
            var breakLabel = Expression.Label( "LoopBreak" );

            if( outLoopInitializations == null )
                outLoopInitializations = Expression.Empty();

            var loop = Expression.Block
            (
                new[] { enumeratorVar },

                enumeratorAssign,
                outLoopInitializations,

                Expression.Loop
                (
                    Expression.IfThenElse
                    (
                        Expression.Equal( moveNextCall, Expression.Constant( true ) ),
                        Expression.Block
                        (
                            new[] { loopVar },

                            Expression.Assign( loopVar, Expression.Property( enumeratorVar, "Current" ) ),
                            loopContent
                        ),

                        Expression.Break( breakLabel )
                    ),

                    breakLabel
               )
            );

            return loop;
        }

        private Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> _mapperFunc;
        public Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> MapperFunc
        {
            get
            {
                if( _mapperFunc != null )
                    return _mapperFunc;

                var sourceType = _propertyMappings.Values.First().SourceProperty.PropertyInfo.DeclaringType;
                var targetType = _propertyMappings.Values.First().TargetProperty.PropertyInfo.DeclaringType;
                var trackerType = typeof( ReferenceTracking );

                var sourceLambdaArg = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetLambdaArg = Expression.Parameter( typeof( object ), "targetInstance" );
                var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

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
                var sourceProperty = new SourceProperty( sourcePropertyInfo )
                {
                    IsBuiltInType = sourcePropertyInfo.PropertyType.IsBuiltInType( true ),
                    IsEnumerable = sourcePropertyInfo.PropertyType.IsEnumerable(),
                    ValueGetterExpr = sourcePropertyInfo.GetGetterExpression()
                };

                propertyMapping = new PropertyMapping( sourceProperty );
                _propertyMappings.Add( typePairKey, propertyMapping );
            }

            propertyMapping.TargetProperty = new TargetProperty( targetPropertyInfo )
            {
                IsBuiltInType = targetPropertyInfo.PropertyType.IsBuiltInType( true ),
                NullableUnderlyingType = Nullable.GetUnderlyingType( targetPropertyInfo.PropertyType ),
                ValueSetterExpr = targetPropertyInfo.GetSetterExpression(),
                ValueGetterExpr = targetPropertyInfo.GetGetterExpression()
            };

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

        public PropertyConfiguration<TSource, TTarget> MapProperty<TSourceProperty, TTargetProperty>(
            Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Expression<Func<TSourceProperty, TTargetProperty>> converter = null )
        {
            var sourcePropertyInfo = sourcePropertySelector.ExtractPropertyInfo();
            var targetPropertyInfo = targetPropertySelector.ExtractPropertyInfo();

            var propertyMapping = base.Map( sourcePropertyInfo, targetPropertyInfo );
            propertyMapping.ValueConverterExp = converter;

            return this;
        }

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

            propertyMapping.ValueConverterExp = converter;
            propertyMapping.TargetProperty.CollectionStrategy = collectionStrategy;

            return this;
        }
    }
}

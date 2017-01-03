using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class TypeMapping
    {
        public readonly GlobalConfiguration GlobalConfiguration;
        public readonly TypePair TypePair;

        /*
         *A source property can be mapped to multiple target properties.
         *
         *A target property can be mapped just once and for that reason 
         *multiple mappings override each other and the last one is used.
         *
         *The target property can be therefore used as the key 
         *of this dictionary
         */
        public readonly Dictionary<PropertyInfo, PropertyMapping> PropertyMappings;

        public LambdaExpression CustomTargetConstructor { get; set; }
        public bool IgnoreConventions { get; set; }

        public HashSet<PropertyInfo> IgnoredSourceProperties { get; private set; }

        public TypeMapping( GlobalConfiguration globalConfig, TypePair typePair )
        {
            this.GlobalConfiguration = globalConfig;
            this.IgnoreConventions = globalConfig.IgnoreConventions;

            this.TypePair = typePair;
            this.PropertyMappings = new Dictionary<PropertyInfo, PropertyMapping>();
            this.IgnoredSourceProperties = new HashSet<PropertyInfo>();
        }

        private LambdaExpression _expression;
        public LambdaExpression MappingExpression
        {
            get
            {
                if( _expression != null ) return _expression;
                if( !this.PropertyMappings.Any() ) return null;

                var returnType = typeof( List<ObjectPair> );
                var returnElementType = typeof( ObjectPair );

                var firstMapping = this.PropertyMappings.Values.First();

                var sourceType = firstMapping.SourceProperty.PropertyInfo.ReflectedType;
                var targetType = firstMapping.TargetProperty.PropertyInfo.ReflectedType;
                var trackerType = typeof( ReferenceTracking );

                var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
                var targetInstance = Expression.Parameter( targetType, "targetInstance" );
                var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

                var newRefObjects = Expression.Variable( returnType, "result" );

                var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var addRangeMethod = returnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );
                var addCalls = PropertyMappings.Values.Select( mapping =>
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
                if( !this.PropertyMappings.Any() ) return null;

                var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );
                var sourceLambdaArg = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetLambdaArg = Expression.Parameter( typeof( object ), "targetInstance" );

                var firstMapping = this.PropertyMappings.Values.First();

                var sourceType = firstMapping.SourceProperty.PropertyInfo.ReflectedType;
                var targetType = firstMapping.TargetProperty.PropertyInfo.ReflectedType;

                var sourceInstance = Expression.Convert( sourceLambdaArg, sourceType );
                var targetInstance = Expression.Convert( targetLambdaArg, targetType );

                var bodyExp = Expression.Invoke( this.MappingExpression,
                    referenceTrack, sourceInstance, targetInstance );

                return _mapperFunc = Expression.Lambda<Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>>>(
                    bodyExp, referenceTrack, sourceLambdaArg, targetLambdaArg ).Compile();
            }
        }

    }
}

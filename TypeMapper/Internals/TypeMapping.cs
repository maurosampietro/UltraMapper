using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Mappers;
using TypeMapper.Mappers.TypeMappers;

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
        public readonly Dictionary<MemberInfo, MemberMapping> MemberMappings;

        public LambdaExpression CustomTargetConstructor { get; set; }
        public bool IgnoreConventions { get; set; }

        public LambdaExpression CustomConverter { get; set; }

        public TypeMapping( GlobalConfiguration globalConfig, TypePair typePair )
        {
            this.GlobalConfiguration = globalConfig;

            if( globalConfig != null )
                this.IgnoreConventions = globalConfig.IgnoreConventions;

            this.TypePair = typePair;
            this.MemberMappings = new Dictionary<MemberInfo, MemberMapping>();
        }

        private LambdaExpression _expression;
        public LambdaExpression MappingExpression
        {
            get
            {
                if( _expression != null ) return _expression;

                var returnType = typeof( List<ObjectPair> );
                var returnElementType = typeof( ObjectPair );

                var sourceType = TypePair.SourceType;
                var targetType = TypePair.TargetType;
                var trackerType = typeof( ReferenceTracking );

                var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
                var targetInstance = Expression.Parameter( targetType, "targetInstance" );
                var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

                var newRefObjects = Expression.Variable( returnType, "result" );

                LambdaExpression typeMappingExp = null;

                if( new CollectionMapperTypeMapping().CanHandle( this ) )
                    typeMappingExp = new CollectionMapperTypeMapping().GetMappingExpression( this );

                var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
                var addRangeMethod = returnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );

                Func<LambdaExpression, Expression> createAddCalls = ( lambdaExp ) =>
                 {
                     if( lambdaExp.ReturnType.ImplementsInterface( typeof( IEnumerable<ObjectPair> ) ) )
                         return Expression.Call( newRefObjects, addRangeMethod, lambdaExp.Body );

                     if( lambdaExp.ReturnType == returnElementType )
                     {
                         var objPair = Expression.Variable( returnElementType, "objPair" );

                         return Expression.Block
                         (
                             new[] { objPair },

                             Expression.Assign( objPair, lambdaExp.Body ),

                             Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
                                 Expression.Call( newRefObjects, addMethod, objPair ) )

                         );
                     }

                     if( lambdaExp.ReturnType == typeof( void ) )
                         return lambdaExp.Body;

                     throw new ArgumentException( "Expressions should return System.Void or ObjectPair or IEnumerable<ObjectPair>" );
                 };

                var addCalls = MemberMappings.Values
                    .Where( mapping => !mapping.SourceProperty.Ignore && !mapping.TargetProperty.Ignore )
                    .Select( mapping => createAddCalls( mapping.Expression ) );

                //var temp = MemberMappings.Values.Select( mapping =>
                // GlobalConfiguration.Configurator[ mapping.SourceProperty.MemberType,
                //     mapping.TargetProperty.MemberType ].MappingExpression ).ToList();

                var bodyExp = (addCalls?.Any() != true) ?
                            (Expression)Expression.Empty() : Expression.Block( addCalls );

                if( typeMappingExp != null )
                {
                    var typeMappingBodyExp = createAddCalls( typeMappingExp );

                    bodyExp = Expression.Block
                    (
                        typeMappingBodyExp,
                        bodyExp
                    );
                }

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

                var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );
                var sourceLambdaArg = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetLambdaArg = Expression.Parameter( typeof( object ), "targetInstance" );

                var sourceType = TypePair.SourceType;
                var targetType = TypePair.TargetType;

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

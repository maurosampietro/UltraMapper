using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Configuration;
using TypeMapper.Mappers;
using TypeMapper.Mappers.TypeMappers;

namespace TypeMapper.Internals
{
    public class TypeMapping
    {
        /*
         *A source member can be mapped to multiple target members.
         *
         *A target member can be mapped just once and for that reason 
         *multiple mappings override each other and the last one is used.
         *
         *The target member can be therefore used as the key of this dictionary
         */
        public readonly Dictionary<MemberInfo, MemberMapping> MemberMappings;
        public readonly GlobalConfiguration GlobalConfiguration;
        public readonly TypePair TypePair;

        public LambdaExpression CustomConverter { get; set; }
        public LambdaExpression CustomTargetConstructor { get; set; }

        private bool? _ignoreMappingResolveByConvention = null;
        public bool IgnoreMappingResolveByConvention
        {
            get
            {
                if( _ignoreMappingResolveByConvention == null )
                    return GlobalConfiguration.IgnoreMappingResolvedByConventions;

                return _ignoreMappingResolveByConvention.Value;
            }

            set { _ignoreMappingResolveByConvention = value; }
        }

        public TypeMapping( GlobalConfiguration globalConfig, TypePair typePair )
        {
            this.GlobalConfiguration = globalConfig;
            this.TypePair = typePair;
            this.MemberMappings = new Dictionary<MemberInfo, MemberMapping>();
        }

        private LambdaExpression _expression;
        public LambdaExpression MappingExpression
        {
            get
            {
                if( _expression != null ) return _expression;
                return _expression = MemberMappingExpressionMerger.Merge( this );
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

    public class MemberMappingExpressionMerger
    {
        private static MemberMappingComparer _memberComparer = new MemberMappingComparer();

        private class MemberMappingComparer : IComparer<MemberMapping>
        {
            public int Compare( MemberMapping x, MemberMapping y )
            {
                var xGetter = x.TargetMember.ValueGetter.ToString();
                var yGetter = y.TargetMember.ValueGetter.ToString();

                int xCount = xGetter.Split( '.' ).Count();
                int yCount = yGetter.Split( '.' ).Count();

                if( xCount > yCount ) return 1;
                if( xCount < yCount ) return -1;

                return 0;
            }
        }

        public static LambdaExpression Merge( TypeMapping typeMapping )
        {
            var returnType = typeof( List<ObjectPair> );
            var returnElementType = typeof( ObjectPair );

            var sourceType = typeMapping.TypePair.SourceType;
            var targetType = typeMapping.TypePair.TargetType;
            var trackerType = typeof( ReferenceTracking );

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );
            var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

            var newRefObjects = Expression.Variable( returnType, "result" );

            var selectedMapper = typeMapping.GlobalConfiguration.Mappers.OfType<ITypeMappingMapperExpression>()
                .FirstOrDefault( mapper => mapper.CanHandle( typeMapping ) );

            var typeMappingExp = selectedMapper?.GetMappingExpression( typeMapping );

            var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
            var addRangeMethod = returnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );

            Func<LambdaExpression, Expression> createAddCalls = ( lambdaExp ) =>
            {
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

                return lambdaExp.Body;
            };

            //since nested selectors are supported, we sort membermappings to grant
            //that we assign outer objects first


            var validMappings = typeMapping.MemberMappings.Values.ToList();
            if( typeMapping.IgnoreMappingResolveByConvention )
                validMappings = validMappings.Where( mapping => mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();

            var addCalls = validMappings.OrderBy( mm => mm, _memberComparer )
                .Where( mapping => !mapping.SourceMember.Ignore && !mapping.TargetMember.Ignore )
                .Select( mapping => createAddCalls( mapping.Expression ) );

            if( !addCalls.Any() && typeMappingExp != null )
                return typeMappingExp;

            var bodyExp = !addCalls.Any() ? (Expression)Expression.Empty() : Expression.Block( addCalls );

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

                bodyExp.ReplaceParameter( referenceTrack, referenceTrack.Name )
                    .ReplaceParameter( targetInstance, targetInstance.Name )
                    .ReplaceParameter( sourceInstance, sourceInstance.Name ),

                newRefObjects
            );

            var delegateType = typeof( Func<,,,> ).MakeGenericType(
                trackerType, sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

            return Expression.Lambda( delegateType,
                body, referenceTrack, sourceInstance, targetInstance );
        }
    }
}

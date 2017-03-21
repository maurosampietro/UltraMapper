using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Configuration;
using TypeMapper.Mappers;
using TypeMapper.Mappers.TypeMappers;

namespace TypeMapper.Internals
{
    public class TypeMapping : ITypeOptions
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

        private bool? _ignoreMappingResolvedByConvention = null;
        public bool IgnoreMemberMappingResolvedByConvention
        {
            get
            {
                if( _ignoreMappingResolvedByConvention == null )
                    return GlobalConfiguration.IgnoreMemberMappingResolvedByConvention;

                return _ignoreMappingResolvedByConvention.Value;
            }

            set { _ignoreMappingResolvedByConvention = value; }
        }

        private ReferenceMappingStrategies? _referenceMappingStrategy;
        public ReferenceMappingStrategies ReferenceMappingStrategy
        {
            get
            {
                if( _referenceMappingStrategy == null )
                    return GlobalConfiguration.ReferenceMappingStrategy;

                return _referenceMappingStrategy.Value;
            }

            set { _referenceMappingStrategy = value; }
        }

        private ICollectionMappingStrategy _collectionMappingStrategy;
        public ICollectionMappingStrategy CollectionMappingStrategy
        {
            get
            {
                if( _collectionMappingStrategy == null )
                    return GlobalConfiguration.CollectionMappingStrategy;

                return _collectionMappingStrategy;
            }

            set { _collectionMappingStrategy = value; }
        }

        public ITypeMapperExpression Mapper
        {
            get
            {
                var selectedMapper = GlobalConfiguration.Mappers.OfType<ITypeMapperExpression>()
                    .FirstOrDefault( mapper => mapper.CanHandle( this.TypePair.SourceType, this.TypePair.TargetType ) );

                return selectedMapper;
            }
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
                if( this.CustomConverter != null )
                    return this.CustomConverter;

                if( _expression != null ) return _expression;
                return _expression = new ReferenceMapperWithMemberMapping().GetMappingExpression( this );
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

    //public class MemberMappingExpressionMerger
    //{
    //    private static MemberMappingComparer _memberComparer = new MemberMappingComparer();

    //    private class MemberMappingComparer : IComparer<MemberMapping>
    //    {
    //        public int Compare( MemberMapping x, MemberMapping y )
    //        {
    //            var xGetter = x.TargetMember.ValueGetter.ToString();
    //            var yGetter = y.TargetMember.ValueGetter.ToString();

    //            int xCount = xGetter.Split( '.' ).Count();
    //            int yCount = yGetter.Split( '.' ).Count();

    //            if( xCount > yCount ) return 1;
    //            if( xCount < yCount ) return -1;

    //            return 0;
    //        }
    //    }

    //    public static LambdaExpression Merge( TypeMapping typeMapping )
    //    {
    //        var returnType = typeof( List<ObjectPair> );
    //        var returnElementType = typeof( ObjectPair );

    //        var sourceType = typeMapping.TypePair.SourceType;
    //        var targetType = typeMapping.TypePair.TargetType;
    //        var trackerType = typeof( ReferenceTracking );

    //        var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
    //        var targetInstance = Expression.Parameter( targetType, "targetInstance" );
    //        var referenceTrack = Expression.Parameter( trackerType, "referenceTracker" );

    //        var newRefObjects = Expression.Variable( returnType, "result" );

    //        LambdaExpression typeMappingExp = typeMapping.Mapper?.GetMappingExpression(
    //            typeMapping.TypePair.SourceType, typeMapping.TypePair.TargetType );

    //        var addMethod = returnType.GetMethod( nameof( List<ObjectPair>.Add ) );
    //        var addRangeMethod = returnType.GetMethod( nameof( List<ObjectPair>.AddRange ) );

    //        Func<MemberMapping, Expression> getMemberMapping = ( mapping ) =>
    //        {
    //            ParameterExpression value = Expression.Parameter( mapping.TargetMember.MemberType, "returnValue" );

    //            var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
    //            var targetSetterMemberParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

    //            LambdaExpression expression = mapping.Expression;

    //            if( mapping.Expression.ReturnType == typeof( List<ObjectPair> ) )
    //            {
    //                return Expression.Call( newRefObjects, addRangeMethod, mapping.Expression.Body );
    //            }

    //            else if( expression.ReturnType == returnElementType )
    //            {
    //                var objPair = Expression.Variable( returnElementType, "objPair" );

    //                //var targetMemberInfo = typeof( ObjectPair ).GetMember( nameof( ObjectPair.Target ) )[ 0 ];
    //                //var targetAccess = Expression.MakeMemberAccess( objPair, targetMemberInfo );

    //                return Expression.Block
    //                (
    //                    new[] { value, objPair },

    //                    Expression.Assign( objPair, expression.Body ),

    //                    //Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
    //                    //Expression.Assign( value, Expression.Convert( targetAccess, mapping.TargetMember.MemberType ) )),

    //                    //mapping.TargetMember.ValueSetter.Body
    //                    //    .ReplaceParameter( targetInstance, targetSetterInstanceParamName )
    //                    //    .ReplaceParameter( value, targetSetterMemberParamName ),

    //                    Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null, returnElementType ) ),
    //                        Expression.Call( newRefObjects, addMethod, objPair ) )
    //                );
    //            }


    //            return Expression.Block
    //            (
    //                new[] { value },

    //                Expression.Assign( value, Expression.Invoke( expression, Expression.Invoke( mapping.SourceMember.ValueGetter, sourceInstance ) ) ),

    //                mapping.TargetMember.ValueSetter.Body
    //                    .ReplaceParameter( targetInstance, targetSetterInstanceParamName )
    //                    .ReplaceParameter( value, targetSetterMemberParamName )
    //            );
    //        };

    //        Func<LambdaExpression, Expression> createAddCalls = ( lambdaExp ) =>
    //        {
    //            if( lambdaExp.ReturnType == returnElementType )
    //            {
    //                var objPair = Expression.Variable( returnElementType, "objPair" );

    //                return Expression.Block
    //                (
    //                    new[] { objPair },

    //                    Expression.Assign( objPair, lambdaExp.Body ),

    //                    Expression.IfThen( Expression.NotEqual( objPair, Expression.Constant( null ) ),
    //                        Expression.Call( newRefObjects, addMethod, objPair ) )
    //                );
    //            }

    //            return lambdaExp.Body;
    //        };

    //        //since nested selectors are supported, we sort membermappings to grant
    //        //that we assign outer objects first

    //        var validMappings = typeMapping.MemberMappings.Values.ToList();
    //        if( typeMapping.IgnoreMemberMappingResolvedByConvention )
    //        {
    //            validMappings = validMappings.Where( mapping =>
    //                mapping.MappingResolution != MappingResolution.RESOLVED_BY_CONVENTION ).ToList();
    //        }

    //        var addCalls = validMappings.OrderBy( mm => mm, _memberComparer )
    //            .Where( mapping => !mapping.SourceMember.Ignore && !mapping.TargetMember.Ignore )
    //            .Select( mapping => getMemberMapping( mapping ) ).ToList();

    //        if( !addCalls.Any() && typeMappingExp != null )
    //            return typeMappingExp;

    //        var bodyExp = !addCalls.Any() ? (Expression)Expression.Empty() : Expression.Block( addCalls );
    //        var typeMappingBodyExp = typeMappingExp != null ? createAddCalls( typeMappingExp ) : Expression.Empty();

    //        var body = Expression.Block
    //        (
    //            new[] { newRefObjects },

    //            Expression.Assign( newRefObjects, Expression.New( returnType ) ),

    //            typeMappingBodyExp.ReplaceParameter( referenceTrack, referenceTrack.Name )
    //                .ReplaceParameter( targetInstance, targetInstance.Name )
    //                .ReplaceParameter( sourceInstance, sourceInstance.Name ),

    //            bodyExp.ReplaceParameter( referenceTrack, referenceTrack.Name )
    //                .ReplaceParameter( targetInstance, targetInstance.Name )
    //                .ReplaceParameter( sourceInstance, sourceInstance.Name ),

    //            newRefObjects
    //        );

    //        var delegateType = typeof( Func<,,,> ).MakeGenericType(
    //            trackerType, sourceType, targetType, typeof( IEnumerable<ObjectPair> ) );

    //        return Expression.Lambda( delegateType,
    //            body, referenceTrack, sourceInstance, targetInstance );
    //    }
    //}
}

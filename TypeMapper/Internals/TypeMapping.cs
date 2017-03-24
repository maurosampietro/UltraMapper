using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Configuration;
using TypeMapper.Mappers;

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
}

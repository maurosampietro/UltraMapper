using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Configuration;
using UltraMapper.Mappers;

namespace UltraMapper.Internals
{
    public class TypeMapping : ITypeOptions, IMapping
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

        public TypeMapping( GlobalConfiguration globalConfig, TypePair typePair )
        {
            this.GlobalConfiguration = globalConfig;
            this.TypePair = typePair;
            this.MemberMappings = new Dictionary<MemberInfo, MemberMapping>();
        }

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

        private CollectionMappingStrategies? _collectionMappingStrategy;
        public CollectionMappingStrategies CollectionMappingStrategy
        {
            get
            {
                if( _collectionMappingStrategy == null )
                    return GlobalConfiguration.CollectionMappingStrategy;

                return _collectionMappingStrategy.Value;
            }

            set { _collectionMappingStrategy = value; }
        }

        private IMapperExpressionBuilder _mapper = null;
        public IMapperExpressionBuilder Mapper
        {
            get
            {
                if( _mapper == null )
                {
                    _mapper = GlobalConfiguration.Mappers.FirstOrDefault(
                        mapper => mapper.CanHandle( this.TypePair.SourceType, this.TypePair.TargetType ) );

                    if( _mapper == null )
                        throw new Exception( $"No object mapper can handle {this}" );
                }

                return _mapper;
            }
        }

        private LambdaExpression _mappingExpression;
        public LambdaExpression MappingExpression
        {
            get
            {
                if( this.CustomConverter != null )
                    return this.CustomConverter;

                if( _mappingExpression != null ) return _mappingExpression;

                return _mappingExpression = this.Mapper.GetMappingExpression(
                    this.TypePair.SourceType, this.TypePair.TargetType, this );
            }
        }

        private Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> _mappingFunc;
        public Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> MappingFunc
        {
            get
            {
                if( _mappingFunc != null ) return _mappingFunc;

                var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );
                var sourceLambdaArg = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetLambdaArg = Expression.Parameter( typeof( object ), "targetInstance" );

                var sourceType = TypePair.SourceType;
                var targetType = TypePair.TargetType;

                var sourceInstance = Expression.Convert( sourceLambdaArg, sourceType );
                var targetInstance = Expression.Convert( targetLambdaArg, targetType );

                var bodyExp = Expression.Invoke( this.MappingExpression,
                    referenceTrack, sourceInstance, targetInstance );

                return _mappingFunc = Expression.Lambda<Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>>>(
                    bodyExp, referenceTrack, sourceLambdaArg, targetLambdaArg ).Compile();
            }
        }
    }
}

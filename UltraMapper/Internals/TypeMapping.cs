using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.ExtensionMethods;
using UltraMapper.Mappers;

namespace UltraMapper.Internals
{
    public sealed class TypeMapping : ITypeOptions, IMapping
    {
        //Each source and target member is instantiated only once per typemapping
        //so we can handle their options/configuration override correctly.
        private readonly Dictionary<MemberInfo, MappingSource> _sourceProperties
            = new Dictionary<MemberInfo, MappingSource>();

        private readonly Dictionary<MemberInfo, MappingTarget> _targetProperties
            = new Dictionary<MemberInfo, MappingTarget>();

        /*
         *A source member can be mapped to multiple target members.
         *
         *A target member can be mapped just once and for that reason 
         *multiple mappings override each other and the last one is used.
         *
         *The target member can be therefore used as the key of this dictionary
         */
        public readonly Dictionary<MappingTarget, MemberMapping> MemberMappings;
        public readonly Configuration GlobalConfiguration;
        public readonly TypePair TypePair;

        public TypeMapping( Configuration globalConfig, TypePair typePair )
        {
            this.GlobalConfiguration = globalConfig;
            this.TypePair = typePair;
            this.MemberMappings = new Dictionary<MappingTarget, MemberMapping>();
        }

        public LambdaExpression CustomConverter { get; set; }
        public LambdaExpression CustomTargetConstructor { get; set; }

        private bool? _ignoreMappingResolvedByConvention;
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

        private IMappingExpressionBuilder _mapper;
        public IMappingExpressionBuilder Mapper
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

        public MappingSource GetMappingSource( MemberInfo sourceMember,
            LambdaExpression sourceMemberGetterExpression )
        {
            return _sourceProperties.GetOrAdd( sourceMember,
               () => new MappingSource( sourceMemberGetterExpression ) );
        }

        public MappingTarget GetMappingTarget( MemberInfo targetMember,
            LambdaExpression targetMemberGetterExpression, LambdaExpression targetMemberSetterExpression )
        {
            return _targetProperties.GetOrAdd( targetMember,
                () => new MappingTarget( targetMemberGetterExpression, targetMemberSetterExpression ) );
        }

        public MemberMapping GetMemberMapping( MappingSource mappingSource, MappingTarget mappingTarget )
        {
            return new MemberMapping( this, mappingSource, mappingTarget );
        }
    }
}

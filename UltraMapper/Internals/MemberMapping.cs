using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Mappers;

namespace UltraMapper.Internals
{
    public enum MappingResolution { RESOLVED_BY_CONVENTION, USER_DEFINED }

    public class MemberMapping : IMemberOptions, IMapping
    {
        private readonly Lazy<string> _toString;

        public readonly TypeMapping InstanceTypeMapping;

        public readonly MappingSource SourceMember;
        public readonly MappingTarget TargetMember;

        public MemberMapping( TypeMapping typeMapping,
            MappingSource sourceMember, MappingTarget targetMember )
        {
            this.InstanceTypeMapping = typeMapping;

            this.SourceMember = sourceMember;
            this.TargetMember = targetMember;

            _toString = new Lazy<string>( () =>
            {
                return $"{this.SourceMember} -> {this.TargetMember}";
            } );
        }

        public MappingResolution MappingResolution { get; internal set; }

        public bool Ignore { get; set; }

        private TypeMapping _memberTypeMapping;
        public TypeMapping MemberTypeMapping
        {
            get
            {
                if( _memberTypeMapping == null )
                {
                    _memberTypeMapping = InstanceTypeMapping.GlobalConfiguration[
                        SourceMember.MemberType, TargetMember.MemberType ];
                }

                return _memberTypeMapping;
            }
        }

        private LambdaExpression _customConverter;
        public LambdaExpression CustomConverter
        {
            get
            {
                if( _customConverter == null )
                    return MemberTypeMapping.CustomConverter;

                return _customConverter;
            }

            set { _customConverter = value; }
        }

        private LambdaExpression _customTargetConstructor;
        public LambdaExpression CustomTargetConstructor
        {
            get
            {
                if( _customTargetConstructor == null )
                    return MemberTypeMapping.CustomTargetConstructor;

                return _customTargetConstructor;
            }

            set { _customTargetConstructor = value; }
        }

        public LambdaExpression CollectionEqualityComparer { get; internal set; }

        private ReferenceMappingStrategies? _referenceMappingStrategy;
        public ReferenceMappingStrategies ReferenceMappingStrategy
        {
            get
            {
                if( _referenceMappingStrategy == null )
                    // return MemberTypeMapping.ReferenceMappingStrategy;
                    return InstanceTypeMapping.ReferenceMappingStrategy;

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
                    //    return MemberTypeMapping.CollectionMappingStrategy;
                    return InstanceTypeMapping.CollectionMappingStrategy;

                return _collectionMappingStrategy.Value;
            }

            set { _collectionMappingStrategy = value; }
        }

        private IMapperExpressionBuilder _mapper;
        public IMapperExpressionBuilder Mapper
        {
            get
            {
                if( _mapper == null )
                {
                    _mapper = InstanceTypeMapping.GlobalConfiguration
                        .Mappers.FirstOrDefault( mapper => mapper.CanHandle(
                            this.MemberTypeMapping.TypePair.SourceType,
                            this.MemberTypeMapping.TypePair.TargetType ) );

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
                    this.MemberTypeMapping.TypePair.SourceType,
                    this.MemberTypeMapping.TypePair.TargetType, this );
            }
        }

        private Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> _mapperFunc;
        public Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>> MappingFunc
        {
            get
            {
                if( _mapperFunc != null ) return _mapperFunc;

                var referenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );
                var sourceLambdaArg = Expression.Parameter( typeof( object ), "sourceInstance" );
                var targetLambdaArg = Expression.Parameter( typeof( object ), "targetInstance" );

                var sourceType = this.MemberTypeMapping.TypePair.SourceType;
                var targetType = this.MemberTypeMapping.TypePair.TargetType;

                var sourceInstance = Expression.Convert( sourceLambdaArg, sourceType );
                var targetInstance = Expression.Convert( targetLambdaArg, targetType );

                var bodyExp = Expression.Invoke( this.MappingExpression,
                    referenceTrack, sourceInstance, targetInstance );

                return _mapperFunc = Expression.Lambda<Func<ReferenceTracking, object, object, IEnumerable<ObjectPair>>>(
                    bodyExp, referenceTrack, sourceLambdaArg, targetLambdaArg ).Compile();
            }
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
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
                $"{this.SourceMember} -> {this.TargetMember}" );
        }

        public MappingResolution MappingResolution { get; internal set; }

        public bool Ignore { get; set; }

        private LambdaExpression _collectionItemEqualityComparer = null;
        public LambdaExpression CollectionItemEqualityComparer
        {
            get
            {
                if( _collectionItemEqualityComparer == null )
                    return MemberTypeMapping.CollectionItemEqualityComparer;

                return _collectionItemEqualityComparer;
            }

            set { _collectionItemEqualityComparer = value; }
        }

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

        private ReferenceBehaviors? _referenceMappingStrategy;
        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                if( _referenceMappingStrategy == null )
                {
                    if( MemberTypeMapping.MappingResolution == MappingResolution.USER_DEFINED )
                        return MemberTypeMapping.ReferenceBehavior;

                    return InstanceTypeMapping.ReferenceBehavior;
                }

                return _referenceMappingStrategy.Value;
            }

            set { _referenceMappingStrategy = value; }
        }

        private CollectionBehaviors? _collectionMappingStrategy;
        public CollectionBehaviors CollectionBehavior
        {
            get
            {
                if( _referenceMappingStrategy == null )
                {
                    if( MemberTypeMapping.MappingResolution == MappingResolution.USER_DEFINED )
                        return MemberTypeMapping.CollectionBehavior;

                    return InstanceTypeMapping.CollectionBehavior;
                }

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

        private Action<ReferenceTracking, object, object> _mapperFunc;
        public Action<ReferenceTracking, object, object> MappingFunc
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

                return _mapperFunc = Expression.Lambda<Action<ReferenceTracking, object, object>>(
                    bodyExp, referenceTrack, sourceLambdaArg, targetLambdaArg ).Compile();
            }
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

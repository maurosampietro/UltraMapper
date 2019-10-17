using System;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Internals
{
    public class MemberMapping : IMemberOptions, IMapping
    {
        public readonly TypeMapping InstanceTypeMapping;
        public readonly MappingSource SourceMember;
        public readonly MappingTarget TargetMember;
        private string _toString;

        public MemberMapping( TypeMapping typeMapping,
            MappingSource sourceMember, MappingTarget targetMember )
        {
            this.InstanceTypeMapping = typeMapping;

            this.SourceMember = sourceMember;
            this.TargetMember = targetMember;
        }

        public MappingResolution MappingResolution { get; internal set; }
        public bool Ignore { get; set; }

        private LambdaExpression _collectionItemEqualityComparer = null;
        public LambdaExpression CollectionItemEqualityComparer
        {
            get { return _collectionItemEqualityComparer ?? MemberTypeMapping.CollectionItemEqualityComparer; }
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
            get { return _customConverter ?? MemberTypeMapping.CustomConverter; }
            set { _customConverter = value; }
        }

        private LambdaExpression _customTargetConstructor;
        public LambdaExpression CustomTargetConstructor
        {
            get { return _customTargetConstructor ?? MemberTypeMapping.CustomTargetConstructor; }
            set { _customTargetConstructor = value; }
        }

        private ReferenceBehaviors? _referenceBehavior;
        public ReferenceBehaviors ReferenceBehavior
        {
            get
            {
                if( _referenceBehavior == null )
                {
                    //if( MemberTypeMapping.MappingResolution == MappingResolution.USER_DEFINED )
                    return MemberTypeMapping.ReferenceBehavior;

                    return InstanceTypeMapping.ReferenceBehavior;
                }

                return _referenceBehavior.Value;
            }

            set { _referenceBehavior = value; }
        }

        public CollectionBehaviors CollectionBehavior { get; set; }

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

                    if( this.CustomConverter == null && _mapper == null )
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

                //if( _mappingExpression != null ) return _mappingExpression;

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
            if( _toString == null )
                _toString = $"{this.SourceMember} -> {this.TargetMember}";

            return _toString;
        }
    }
}

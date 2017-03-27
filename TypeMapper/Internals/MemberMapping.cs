using System;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.CollectionMappingStrategies;
using TypeMapper.Mappers;

namespace TypeMapper.Internals
{
    public enum MappingResolution { RESOLVED_BY_CONVENTION, USER_DEFINED }

    public class MemberMapping : IMappingOptions
    {
        private readonly Lazy<string> _toString;

        public readonly TypeMapping InstanceTypeMapping;

        public readonly MappingSource SourceMember;
        public readonly MappingTarget TargetMember;

        public MappingResolution MappingResolution { get; internal set; }
        public IMapperExpressionBuilder Mapper
        {
            get
            {
                var selectedMapper = InstanceTypeMapping.GlobalConfiguration
                    .Mappers.FirstOrDefault( mapper => mapper.CanHandle( 
                        this.MemberTypeMapping.TypePair.SourceType,
                        this.MemberTypeMapping.TypePair.TargetType ) );

                if( selectedMapper == null )
                    throw new Exception( $"No object mapper can handle {this}" );

                return selectedMapper;
            }
        }

        public TypeMapping MemberTypeMapping
        {
            get
            {
                return InstanceTypeMapping.GlobalConfiguration.Configuration[
                    SourceMember.MemberType, TargetMember.MemberType ];
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

        private ICollectionMappingStrategy _collectionMappingStrategy;
        public ICollectionMappingStrategy CollectionMappingStrategy
        {
            get
            {
                if( _collectionMappingStrategy == null )
                    return MemberTypeMapping.CollectionMappingStrategy;

                return _collectionMappingStrategy;
            }

            set { _collectionMappingStrategy = value; }
        }

        private ReferenceMappingStrategies? _referenceMappingStrategy;
        public ReferenceMappingStrategies ReferenceMappingStrategy
        {
            get
            {
                if( _referenceMappingStrategy == null )
                    return MemberTypeMapping.ReferenceMappingStrategy;

                return _referenceMappingStrategy.Value;
            }

            set { _referenceMappingStrategy = value; }
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
                    this.MemberTypeMapping.TypePair.TargetType );
            }
        }

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

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

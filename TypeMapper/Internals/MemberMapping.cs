using System;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Mappers;

namespace TypeMapper.Internals
{
    public enum MappingResolution { RESOLVED_BY_CONVENTION, USER_DEFINED }

    public class MemberMapping
    {
        private readonly Lazy<string> _toString;

        public readonly TypeMapping InstanceTypeMapping;

        public readonly MappingSource SourceMember;
        public readonly MappingTarget TargetMember;

        public MappingResolution MappingResolution { get; internal set; }
        public IMemberMappingMapperExpression Mapper
        {
            get
            {
                var selectedMapper = InstanceTypeMapping.GlobalConfiguration.Mappers.FirstOrDefault(
                    mapper => mapper.CanHandle( this ) );

                if( selectedMapper == null )
                    throw new Exception( $"No object mapper can handle {this}" );

                return selectedMapper;
            }
        }

        public TypeMapping MemberTypeMapping
        {
            get
            {
                return InstanceTypeMapping.GlobalConfiguration.Configurator[
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

        public LambdaExpression Expression
        {
            get { return this.Mapper.GetMappingExpression( this ); }
        }

        public LambdaExpression CollectionEqualityComparer { get; internal set; }

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

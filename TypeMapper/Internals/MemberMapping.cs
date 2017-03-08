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

        //public readonly MemberInfoPair PropertyInfoPair;
        public readonly MappingSource SourceProperty;
        public readonly MappingTarget TargetProperty;

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
                            SourceProperty.MemberType, TargetProperty.MemberType ];
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

        public MemberMapping( TypeMapping typeMapping,
            MappingSource sourceProperty, MappingTarget targetProperty )
        {
            this.InstanceTypeMapping = typeMapping;

            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;

            //this.PropertyInfoPair = new MemberInfoPair(
            //   this.SourceProperty.MemberInfo, this.TargetProperty.MemberInfo );

            _toString = new Lazy<string>( () =>
            {
                return $"{this.SourceProperty} -> {this.TargetProperty}";
            } );
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

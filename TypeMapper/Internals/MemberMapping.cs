using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Mappers;

namespace TypeMapper.Internals
{
    public enum MappingResolution { RESOLVED_BY_CONVENTION, USER_DEFINED }

    public class MemberMapping
    {
        private readonly Lazy<string> _toString;

        public readonly TypeMapping TypeMapping;
        public readonly MemberInfoPair PropertyInfoPair;
        public readonly MappingSource SourceProperty;
        public readonly MappingTarget TargetProperty;

        public MappingResolution MappingResolution { get; internal set; }
        public IObjectMapperExpression Mapper { get; set; }

        public LambdaExpression CustomConverter { get; set; }
        public LambdaExpression Expression
        {
            get { return this.Mapper.GetMappingExpression(this); }
        }

        public MemberMapping(TypeMapping typeMapping,
            MappingSource sourceProperty, MappingTarget targetProperty)
        {
            this.TypeMapping = typeMapping;

            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;

            this.PropertyInfoPair = new MemberInfoPair(
               this.SourceProperty.MemberInfo, this.TargetProperty.MemberInfo);

            _toString = new Lazy<string>(() =>
           {
               return $"{this.SourceProperty} -> {this.TargetProperty}";
           });
        }

        //public PropertyMapping( TypeMapping typeMapping,
        //  PropertyInfo sourcePropertySelector,
        //  PropertyInfo targetPropertySelector )
        //{
        //    this.TypeMapping = typeMapping;

        //    this.SourceProperty = new SourceProperty( sourcePropertySelector );
        //    this.TargetProperty = new TargetProperty( targetPropertySelector );

        //    this.PropertyInfoPair = new PropertyInfoPair(
        //       this.SourceProperty.PropertyInfo, this.TargetProperty.PropertyInfo );

        //    _toString = new Lazy<string>( () =>
        //        {
        //            return $"{this.SourceProperty} -> {this.TargetProperty}";
        //        } );
        //}

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

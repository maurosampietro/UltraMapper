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

    public class PropertyMapping
    {
        private readonly Lazy<string> _toString;

        public readonly TypeMapping TypeMapping;
        public readonly PropertyInfoPair PropertyInfoPair;
        public readonly SourceProperty SourceProperty;
        public readonly TargetProperty TargetProperty;

        public MappingResolution MappingResolution { get; internal set; }
        public IObjectMapperExpression Mapper { get; set; }

        public LambdaExpression CustomConverter { get; set; }
        public LambdaExpression Expression
        {
            get { return this.Mapper.GetMappingExpression( this ); }
        }

        public PropertyMapping( TypeMapping typeMapping,
            PropertyInfo sourceProperty, PropertyInfo targetProperty )
        {
            this.TypeMapping = typeMapping;

            this.PropertyInfoPair = new PropertyInfoPair( 
                sourceProperty, targetProperty );

            this.SourceProperty = new SourceProperty( sourceProperty );
            this.TargetProperty = new TargetProperty( targetProperty );

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

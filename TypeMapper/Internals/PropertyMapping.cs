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
    public class PropertyMapping
    {
        private readonly Lazy<string> _toString;

        public readonly SourceProperty SourceProperty;
        public readonly TargetProperty TargetProperty;

        public IObjectMapperExpression Mapper { get; set; }
        public LambdaExpression CustomConverter { get; set; }

        public LambdaExpression Expression
        {
            get { return this.Mapper.GetMappingExpression( this ); }
        }

        public PropertyMapping( PropertyInfo sourceProperty, PropertyInfo targetProperty )
        {
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

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
        public SourceProperty SourceProperty { get; private set; }
        public TargetProperty TargetProperty { get; set; }
        public IObjectMapperExpression Mapper { get; set; }
        public LambdaExpression ValueConverterExp { get; set; }

        public Expression Expression
        {
            get { return this.Mapper.GetMappingExpression( this ); }
        }

        public PropertyMapping( SourceProperty sourceProperty,
            TargetProperty targetProperty = null,
            LambdaExpression converterExp = null )
        {
            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;
            this.ValueConverterExp = converterExp;
        }

        public override string ToString()
        {
            return $"{this.SourceProperty} -> {this.TargetProperty}";
        }
    }
}

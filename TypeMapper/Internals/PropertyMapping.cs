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
        public LambdaExpression CustomConverter { get; set; }

        private LambdaExpression _expression;
        public LambdaExpression Expression
        {
            get
            {
                if( _expression == null )
                    _expression = this.Mapper.GetMappingExpression( this );

                return _expression;
            }
        }

        public PropertyMapping( SourceProperty sourceProperty,
            TargetProperty targetProperty = null,
            LambdaExpression converterExp = null )
        {
            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;
            this.CustomConverter = converterExp;
        }

        public override string ToString()
        {
            return $"{this.SourceProperty} -> {this.TargetProperty}";
        }
    }
}

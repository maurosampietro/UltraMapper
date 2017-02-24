using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers.MapperContexts
{
    public class CustomConverterContext : MapperContext
    {
        public LambdaExpression CustomConverter { get; protected set; }

        public CustomConverterContext( MemberMapping mapping )
            : base( mapping )
        {
            this.CustomConverter = mapping.CustomConverter;
        }
    }
}

using System.Linq.Expressions;
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

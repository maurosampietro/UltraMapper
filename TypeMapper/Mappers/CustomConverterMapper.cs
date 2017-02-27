using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class CustomConverterMapper : BaseMapper, IObjectMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            return mapping.CustomConverter != null;
        }

        protected override MapperContext GetContext( MemberMapping mapping )
        {
            return new CustomConverterContext( mapping );
        }

        protected override Expression GetTargetValueAssignment( MapperContext context )
        {
            var converterContext = context as CustomConverterContext;

            var value = Expression.Invoke( converterContext.CustomConverter,
                    converterContext.SourceMemberValue );

            return Expression.Assign( converterContext.TargetMember, value );
        }
    }
}

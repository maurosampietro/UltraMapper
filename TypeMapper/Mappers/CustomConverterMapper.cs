using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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

        protected override Expression GetValueAssignment( MapperContext context )
        {
            var converterContext = context as CustomConverterContext;

            var value = Expression.Invoke( converterContext.CustomConverter,
                    converterContext.SourceValue );

            return Expression.Assign( converterContext.TargetValue, value );
        }
    }
}

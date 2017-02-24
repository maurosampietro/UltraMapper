using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class CustomConverterMapper : BaseMapper, IObjectMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            return mapping.CustomConverter != null;
        }

        protected override Expression GetValueAssignment( MapperContext context )
        {
            var sourceGetterInstanceParamName = context.Mapping.SourceProperty
                .ValueGetter.Parameters[ 0 ].Name;

            var readValueExp = context.Mapping.SourceProperty.ValueGetter.Body
                .ReplaceParameter( context.SourceInstance, sourceGetterInstanceParamName );

            return Expression.Assign( context.TargetValue,
                Expression.Invoke( context.Mapping.CustomConverter, readValueExp ) );
        }
    }
}

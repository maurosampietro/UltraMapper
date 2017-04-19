using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.MappingExpressionBuilders
{
    public class StructMapper : PrimitiveMapperBase
    {
        public StructMapper( Configuration configuration )
            : base( configuration ){ }

        public override bool CanHandle( Type sourceType, Type targetType )
        {
            return sourceType.IsValueType && targetType.IsValueType;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            return context.SourceInstance;
        }
    }
}

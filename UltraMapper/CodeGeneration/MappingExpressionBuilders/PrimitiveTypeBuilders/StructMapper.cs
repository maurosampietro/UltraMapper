using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class StructMapper : PrimitiveMapperBase
    {
        public StructMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return source.EntryType.IsValueType && target.EntryType.IsValueType;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            return context.SourceInstance;
        }
    }
}

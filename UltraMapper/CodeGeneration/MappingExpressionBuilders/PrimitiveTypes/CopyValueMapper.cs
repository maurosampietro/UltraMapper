using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public sealed class CopyValueMapper : PrimitiveMapperBase
    {
        public override bool CanHandle( Mapping mapping )
        {
            return mapping.Source.ReturnType.IsBuiltIn( true ) &&
                mapping.Source.ReturnType == mapping.Target.ReturnType;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            return context.SourceInstance;
        }
    }
}

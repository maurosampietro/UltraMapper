using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class EnumMapper : PrimitiveMapperBase
    {
        public EnumMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return target.EntryType.IsEnum;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            return Expression.Convert( Expression.Convert(
                context.SourceInstance, typeof( int ) ), context.TargetInstance.Type );
        }
    }
}

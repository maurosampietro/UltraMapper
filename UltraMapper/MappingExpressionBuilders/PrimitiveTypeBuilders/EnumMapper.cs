using System;
using System.Linq.Expressions;

namespace UltraMapper.MappingExpressionBuilders
{
    public class EnumMapper : PrimitiveMapperBase
    {
        public EnumMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type sourceType, Type targetType )
        {
            return targetType.IsEnum;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            return Expression.Convert( Expression.Convert(
                context.SourceInstance, typeof( int ) ), context.TargetInstance.Type );
        }
    }
}

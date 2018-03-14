using System;
using System.Linq.Expressions;

namespace UltraMapper.MappingExpressionBuilders
{
    public class StringToEnumMapper : PrimitiveMapperBase
    {
        public StringToEnumMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type sourceType, Type targetType )
        {
            return sourceType == typeof( string ) && targetType.IsEnum;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            var enumParseCall = Expression.Call( typeof( Enum ), nameof( Enum.Parse ), null,
                Expression.Constant( context.TargetInstance.Type ), context.SourceInstance, Expression.Constant( true ) );

            return Expression.Convert( enumParseCall, context.TargetInstance.Type );
        }
    }

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

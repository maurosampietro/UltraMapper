using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class StringToEnumMapper : PrimitiveMapperBase
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            return source.EntryType == typeof( string ) && target.EntryType.IsEnum;
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            var enumParseCall = Expression.Call( typeof( Enum ), nameof( Enum.Parse ), null,
                Expression.Constant( context.TargetInstance.Type ), context.SourceInstance, Expression.Constant( true ) );

            return Expression.Convert( enumParseCall, context.TargetInstance.Type );
        }
    }
}

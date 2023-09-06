using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class NullableMapper : PrimitiveMapperBase
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;
            return source.EntryType.IsNullable() || target.EntryType.IsNullable();
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            if( context.SourceInstance.Type == context.TargetInstance.Type )
                return context.SourceInstance;

            var sourceNullUnwrappedType = context.SourceInstance.Type.GetUnderlyingTypeIfNullable();
            var targetNullUnwrappedType = context.TargetInstance.Type.GetUnderlyingTypeIfNullable();

            var labelTarget = Expression.Label( context.TargetInstance.Type, "returnTarget" );

            Expression returnValue = Expression.Convert( context.SourceInstance, sourceNullUnwrappedType );

            if( sourceNullUnwrappedType != targetNullUnwrappedType )
            {
                var conversionlambda = context.MapperConfiguration[ sourceNullUnwrappedType,
                    targetNullUnwrappedType ].MappingExpression;

                returnValue = conversionlambda.Body
                    .ReplaceParameter( returnValue, conversionlambda.Parameters[ 0 ].Name );
            }

            if( context.TargetInstance.Type.IsNullable() )
            {
                if( sourceNullUnwrappedType != targetNullUnwrappedType )
                {
                    var conversionlambda = context.MapperConfiguration[ sourceNullUnwrappedType,
                        targetNullUnwrappedType ].MappingExpression;

                    returnValue = Expression.Convert( conversionlambda.Body
                        .ReplaceParameter( returnValue, conversionlambda.Parameters[ 0 ].Name ), context.TargetInstance.Type );
                }
                else
                {
                    returnValue = Expression.Convert( context.SourceInstance, context.TargetInstance.Type );
                }
            }
            else
            {
                returnValue = Expression.Convert( returnValue, targetNullUnwrappedType );
            }

            if( context.SourceInstance.Type.CanBeSetNull() )
            {
                return Expression.Block
                (
                    Expression.IfThen
                    (
                        Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                        Expression.Return( labelTarget, Expression.Default( context.TargetInstance.Type ) )
                    ),

                    Expression.Label( labelTarget, returnValue )
                );
            }

            return returnValue;
        }
    }
}

using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class NullableMapper : PrimitiveMapperBase
    {
        public NullableMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source.IsNullable() || target.IsNullable();
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            if( context.SourceInstance.Type == context.TargetInstance.Type )
                return context.SourceInstance;

            var sourceNullUnwrappedType = context.SourceInstance.Type.GetUnderlyingTypeIfNullable();
            var targetNullUnwrappedType = context.TargetInstance.Type.GetUnderlyingTypeIfNullable();

            var labelTarget = Expression.Label( context.TargetInstance.Type, "returnTarget" );

            Expression sourceNullTest = Expression.Empty();
            if( context.SourceInstance.Type.IsNullable() )
            {
                sourceNullTest = Expression.IfThen
                (
                    Expression.Equal( context.SourceInstance, Expression.Constant( null, context.SourceInstance.Type ) ),
                    Expression.Return( labelTarget, Expression.Default( context.TargetInstance.Type ) )
                );
            }

            LambdaExpression convert = null;
            if( sourceNullUnwrappedType != targetNullUnwrappedType )
            {
                convert = MapperConfiguration[ sourceNullUnwrappedType,
                    targetNullUnwrappedType ].MappingExpression;
            }

            Expression returnValue = Expression.Convert( context.SourceInstance, sourceNullUnwrappedType );
            if( context.TargetInstance.Type.IsNullable() )
            {
                var constructor = context.TargetInstance.Type
                    .GetConstructor( new Type[] { targetNullUnwrappedType } );

                Expression sourceValue = convert == null ? context.SourceInstance :
                   convert.Body.ReplaceParameter( returnValue, convert.Parameters[ 0 ].Name );

                returnValue = Expression.New( constructor, sourceValue );
            }
            else
            {
                returnValue = convert == null ? Expression.Convert( context.SourceInstance, targetNullUnwrappedType ) :
                    convert.Body.ReplaceParameter( returnValue, convert.Parameters[ 0 ].Name );
            }

            return Expression.Block
            (
                sourceNullTest,
                Expression.Label( labelTarget, returnValue )
            );
        }
    }
}

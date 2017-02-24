using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public sealed class BuiltInTypeMapper : BaseMapper, IObjectMapperExpression, IMapperExpression
    {
        public bool CanHandle( MemberMapping mapping )
        {
            var sourcePropertyType = mapping.SourceProperty.MemberInfo.GetMemberType();
            var targetPropertyType = mapping.TargetProperty.MemberInfo.GetMemberType();

            return this.CanHandle( sourcePropertyType, targetPropertyType );
        }

        public bool CanHandle( Type source, Type target )
        {
            bool areTypesBuiltIn = source.IsBuiltInType( false )
                && target.IsBuiltInType( false );

            return (areTypesBuiltIn) && (source == target ||
                    source.IsImplicitlyConvertibleTo( target ) ||
                    source.IsExplicitlyConvertibleTo( target ));
        }

        public LambdaExpression GetMappingExpression( Type sourceType, Type targetType )
        {
            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );

            var value = Expression.Variable( targetType, "value" );

            Func<Expression> getValueExp = () =>
            {
                if( sourceType == targetType )
                    return Expression.Assign( value, sourceInstance );

                var conversionExp = Expression.Convert(
                    sourceInstance, targetType );

                return Expression.Assign( value, conversionExp );
            };

            var body = Expression.Block( new[] { value }, getValueExp() );

            var delegateType = typeof( Func<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType, body, sourceInstance );
        }

        protected override Expression GetValueAssignment( MapperContext context )
        {
            var readValueExp = context.Mapping.SourceProperty.ValueGetter.Body
                .ReplaceParameter(context.SourceInstance, context.Mapping.SourceProperty.ValueGetter.Parameters[ 0 ].Name );

            if( context.SourcePropertyType == context.TargetPropertyType )
                return Expression.Assign( context.TargetValue, readValueExp );

            var conversionExp = Expression.Convert(
                readValueExp, context.TargetPropertyType );

            return Expression.Assign( context.TargetValue, conversionExp );
        }
    }
}

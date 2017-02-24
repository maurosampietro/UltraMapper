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

        protected override Expression GetValueAssignment( MapperContext context )
        {
            if( context.SourceValueType == context.TargetValueType )
                return Expression.Assign( context.TargetValue, context.SourceValue );

            var conversionExp = Expression.Convert(
                context.SourceValue, context.TargetValueType );

            return Expression.Assign( context.TargetValue, conversionExp );
        }
    }
}

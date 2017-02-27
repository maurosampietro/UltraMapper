using System;
using System.Linq.Expressions;
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

        protected override Expression GetTargetValueAssignment( MapperContext context )
        {
            if( context.SourceMemberType == context.TargetMemberType )
                return Expression.Assign( context.TargetMember, context.SourceMemberValue );

            var conversionExp = Expression.Convert(
                context.SourceMemberValue, context.TargetMemberType );

            return Expression.Assign( context.TargetMember, conversionExp );
        }
    }
}

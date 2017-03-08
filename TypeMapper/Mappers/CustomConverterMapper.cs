using System;
using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class CustomConverterMapper : BaseMapper, IMemberMappingMapperExpression, ITypeMappingMapperExpression
    {
        public override bool CanHandle( MemberMapping mapping )
        {
            return mapping.CustomConverter != null;
        }

        public override bool CanHandle( TypeMapping mapping )
        {
            return mapping.CustomConverter != null;
        }

        public override bool CanHandle( Type sourceType, Type targetType )
        {
            //can't do anything about this, except maybe a lookup to get the typemapping
            return false;
        }

        protected override MapperContext GetContext( MemberMapping mapping )
        {
            return new CustomConverterContext( mapping );
        }

        protected override MapperContext GetContext( TypeMapping mapping )
        {
            return new CustomConverterContext( mapping );
        }

        protected override Expression GetTargetValueAssignment( MapperContext context )
        {
            var converterContext = context as CustomConverterContext;

            var value = Expression.Invoke( converterContext.CustomConverter,
                    converterContext.SourceMemberValue );

            return Expression.Assign( converterContext.TargetMember, value );
        }
    }
}

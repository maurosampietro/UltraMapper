using System;
using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    //public class CustomConverterMapper : BaseMapper, ITypeMappingMapperExpression
    //{
    //    public override bool CanHandle( TypeMapping mapping )
    //    {
    //        return mapping.CustomConverter != null;
    //    }

    //    public override bool CanHandle( Type sourceType, Type targetType )
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override MapperContext GetContext( TypeMapping mapping )
    //    {
    //        return new CustomConverterContext( mapping );
    //    }

    //    protected override Expression GetTargetValueAssignment( MapperContext context )
    //    {
    //        var converterContext = context as CustomConverterContext;

    //        var value = Expression.Invoke( converterContext.CustomConverter,
    //                converterContext.SourceMemberValue );

    //        return Expression.Assign( converterContext.TargetMember, value );
    //    }
    //}
}

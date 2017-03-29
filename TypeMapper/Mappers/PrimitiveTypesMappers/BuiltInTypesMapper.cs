using System;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public sealed class BuiltInTypeMapper : PrimitiveMapperBase
    {
        public BuiltInTypeMapper( MapperConfiguration configuration ) 
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            bool areTypesBuiltIn = source.IsBuiltInType( false )
                && target.IsBuiltInType( false );

            return (areTypesBuiltIn) && (source == target ||
                    source.IsImplicitlyConvertibleTo( target ) ||
                    source.IsExplicitlyConvertibleTo( target ));
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            if( context.SourceInstance.Type == context.TargetInstance.Type )
                return context.SourceInstance;

            var conversionExp = Expression.Convert(
                context.SourceInstance, context.TargetInstance.Type );

            return conversionExp;
        }
    }
}

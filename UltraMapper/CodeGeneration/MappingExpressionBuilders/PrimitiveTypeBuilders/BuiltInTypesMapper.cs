using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public sealed class BuiltInTypeMapper : PrimitiveMapperBase
    {
        public BuiltInTypeMapper( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source;
            var target = mapping.Target;

            bool areTypesBuiltIn = source.EntryType.IsBuiltIn( false )
                && target.EntryType.IsBuiltIn( false );

            return (areTypesBuiltIn ) && (source == target ||
                source.EntryType.IsImplicitlyConvertibleTo( target.EntryType ) ||
                source.EntryType.IsExplicitlyConvertibleTo( target.EntryType ));
        }

        protected override Expression GetValueExpression( MapperContext context )
        {
            if( context.SourceInstance.Type == context.TargetInstance.Type )
                return context.SourceInstance;

            return Expression.Convert( context.SourceInstance,
                context.TargetInstance.Type );
        }
    }
}

using System;
using System.Linq.Expressions;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CollectionMapperViaTempCollectionContext : CollectionMapperContext
    {
        public ParameterExpression TempCollection { get; set; }
        public ParameterExpression TempTargetCollectionLoopingVar { get; }

        public CollectionMapperViaTempCollectionContext( Type source, Type target, IMappingOptions options )
            : base( source, target, options )
        {
            TempTargetCollectionLoopingVar = Expression.Parameter( TargetCollectionElementType, "loopVar" );
        }
    }
}

using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CollectionMapperContext : ReferenceMapperContext
    {
        public Type SourceCollectionElementType { get; protected set; }
        public Type TargetCollectionElementType { get; protected set; }

        public bool IsSourceElementTypeBuiltIn { get; protected set; }
        public bool IsTargetElementTypeBuiltIn { get; protected set; }

        public ParameterExpression SourceCollectionLoopingVar { get; set; }

        public LabelTarget Continue { get; }
        public LabelTarget Break { get; }

        public CollectionMapperContext( Mapping mapping ) : base( mapping )
        {
            SourceCollectionElementType = SourceInstance.Type.GetCollectionGenericType();
            TargetCollectionElementType = TargetInstance.Type.GetCollectionGenericType();

            if( SourceCollectionElementType != null )
            {
                IsSourceElementTypeBuiltIn = SourceCollectionElementType.IsBuiltIn( true );
                SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
            }

            if( TargetCollectionElementType != null )
                IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltIn( true );

            Continue = Expression.Label( "LoopContinue" );
            Break = Expression.Label( "LoopBreak" );
        }
    }
}

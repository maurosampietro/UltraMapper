using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class CollectionMapperContext : ReferenceMapperContext
    {
        public Type SourceCollectionElementType { get; set; }
        public Type TargetCollectionElementType { get; set; }

        public bool IsSourceElementTypeBuiltIn { get; set; }
        public bool IsTargetElementTypeBuiltIn { get; set; }

        public ParameterExpression SourceCollectionLoopingVar { get; set; }

        public CollectionMapperContext( MemberMapping mapping )
            : base( mapping ) { Initialize(); }

        public CollectionMapperContext( Type source, Type target )
            : base( source, target ) { Initialize(); }

        private void Initialize()
        {
            SourceCollectionElementType = SourceMemberType.GetCollectionGenericType();
            TargetCollectionElementType = TargetMemberType.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceCollectionElementType.IsBuiltInType( true );
            IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltInType( true );

            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
        }
    }
}

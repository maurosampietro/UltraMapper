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
            : base( mapping )
        {
            SourceCollectionElementType = SourceMemberType.GetCollectionGenericType();
            TargetCollectionElementType = TargetMemberType.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceMemberType.IsBuiltInType( true );
            IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltInType( true );

            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
        }
    }

    public class CollectionMapperContextTypeMapping : ReferenceMapperContextTypeMapping
    {
        public Type SourceCollectionElementType { get; set; }
        public Type TargetCollectionElementType { get; set; }

        public bool IsSourceElementTypeBuiltIn { get; set; }
        public bool IsTargetElementTypeBuiltIn { get; set; }

        public ParameterExpression SourceCollectionLoopingVar { get; set; }

        public CollectionMapperContextTypeMapping( TypeMapping mapping )
            : base( mapping )
        {
            //new properties
            SourceCollectionElementType = SourcePropertyType.GetCollectionGenericType();
            TargetCollectionElementType = TargetPropertyType.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceCollectionElementType.IsBuiltInType( false );
            IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltInType( false );

            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
        }
    }
}

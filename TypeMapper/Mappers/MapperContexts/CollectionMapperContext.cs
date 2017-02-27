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
        public Type ReturnElementType { get; set; }

        public bool IsSourceElementTypeBuiltIn { get; set; }
        public bool IsTargetElementTypeBuiltIn { get; set; }

        public ParameterExpression SourceCollectionLoopingVar { get; set; }

        public CollectionMapperContext( MemberMapping mapping )
            : base( mapping )
        {
            //base values override
            ReturnType = typeof( List<ObjectPair> );
            ReturnElementType = typeof( ObjectPair );
            ReturnObject = Expression.Variable( ReturnType, "result" );

            //new properties
            SourceCollectionElementType = SourceMemberType.GetCollectionGenericType();
            TargetCollectionElementType = TargetMemberType.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceMemberType.IsBuiltInType( true );
            IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltInType( true );

            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
        }
    }

    public class CollectionMapperContextTypeMapping : ReferenceMapperContextTypeMapping
    {
        public Type SourceElementType { get; set; }
        public Type TargetElementType { get; set; }
        public Type ReturnElementType { get; set; }

        public bool IsSourceElementTypeBuiltIn { get; set; }
        public bool IsTargetElementTypeBuiltIn { get; set; }

        public ParameterExpression SourceLoopingVar { get; set; }

        public CollectionMapperContextTypeMapping( TypeMapping mapping )
            : base( mapping )
        {
            //base values override
            ReturnType = typeof( List<ObjectPair> );
            ReturnElementType = typeof( ObjectPair );
            ReturnObjectVar = Expression.Variable( ReturnType, "result" );

            //new properties
            SourceElementType = SourcePropertyType.GetCollectionGenericType();
            TargetElementType = TargetPropertyType.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceElementType.IsBuiltInType( false );
            IsTargetElementTypeBuiltIn = TargetElementType.IsBuiltInType( false );

            SourceLoopingVar = Expression.Parameter( SourceElementType, "loopVar" );
        }
    }
}

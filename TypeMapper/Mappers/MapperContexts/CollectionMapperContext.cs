using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public CollectionMapperContext( Type source, Type target )
            : base( source, target )
        {
            //ReturnTypeConstructor = ReturnObject.Type.GetConstructor( new[] { typeof( int ) } );

            SourceCollectionElementType = SourceInstance.Type.GetCollectionGenericType();
            TargetCollectionElementType = TargetInstance.Type.GetCollectionGenericType();

            IsSourceElementTypeBuiltIn = SourceCollectionElementType.IsBuiltInType( true );
            IsTargetElementTypeBuiltIn = TargetCollectionElementType.IsBuiltInType( true );

            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
        }
    }
}

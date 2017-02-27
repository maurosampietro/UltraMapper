using System;
using System.Collections;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class DictionaryMapperContext : CollectionMapperContext
    {
        public Type SourceCollectionElementKeyType { get; private set; }
        public Type SourceCollectionElementValueType { get; private set; }
        public Type TargetCollectionElementKeyType { get; private set; }
        public Type TargetCollectionElementValueType { get; private set; }

        public MemberExpression SourceCollectionElementKey { get; private set; }
        public MemberExpression SourceCollectionElementValue { get; private set; }

        public ParameterExpression TargetCollectionElementKey { get; private set; }
        public ParameterExpression TargetCollectionElementValue { get; private set; }

        public DictionaryMapperContext( MemberMapping mapping )
            : base( mapping )
        {
            SourceCollectionElementKeyType = SourceCollectionElementType.GetGenericArguments()[ 0 ];
            SourceCollectionElementValueType = SourceCollectionElementType.GetGenericArguments()[ 1 ];

            TargetCollectionElementKeyType = TargetCollectionElementType.GetGenericArguments()[ 0 ];
            TargetCollectionElementValueType = TargetCollectionElementType.GetGenericArguments()[ 1 ];

            SourceCollectionElementKey = Expression.Property( SourceCollectionLoopingVar, nameof( DictionaryEntry.Key ) );
            SourceCollectionElementValue = Expression.Property( SourceCollectionLoopingVar, nameof( DictionaryEntry.Value ) );

            TargetCollectionElementKey = Expression.Variable( TargetCollectionElementKeyType, "targetKey" );
            TargetCollectionElementValue = Expression.Variable( TargetCollectionElementValueType, "targetValue" );
        }
    }
}

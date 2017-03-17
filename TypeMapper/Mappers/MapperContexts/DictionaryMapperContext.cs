using System;
using System.Collections;
using System.Linq.Expressions;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class DictionaryMapperContext : CollectionMapperContext
    {  
        public MemberExpression SourceCollectionElementKey { get; private set; }
        public MemberExpression SourceCollectionElementValue { get; private set; }

        public ParameterExpression TargetCollectionElementKey { get; private set; }
        public ParameterExpression TargetCollectionElementValue { get; private set; }

        public DictionaryMapperContext( MemberMapping mapping )
            : base( mapping )
        {
            var sourceCollectionElementKeyType = SourceCollectionElementType.GetGenericArguments()[ 0 ];
            var sourceCollectionElementValueType = SourceCollectionElementType.GetGenericArguments()[ 1 ];

            var targetCollectionElementKeyType = TargetCollectionElementType.GetGenericArguments()[ 0 ];
            var targetCollectionElementValueType = TargetCollectionElementType.GetGenericArguments()[ 1 ];

            SourceCollectionElementKey = Expression.Property( SourceCollectionLoopingVar, nameof( DictionaryEntry.Key ) );
            SourceCollectionElementValue = Expression.Property( SourceCollectionLoopingVar, nameof( DictionaryEntry.Value ) );

            TargetCollectionElementKey = Expression.Variable( targetCollectionElementKeyType, "targetKey" );
            TargetCollectionElementValue = Expression.Variable( targetCollectionElementValueType, "targetValue" );
        }

        public DictionaryMapperContext( Type source, Type target )
            : base( source, target )
        {
            var sourceCollectionElementKeyType = SourceCollectionElementType.GetGenericArguments()[ 0 ];
            var sourceCollectionElementValueType = SourceCollectionElementType.GetGenericArguments()[ 1 ];

            var targetCollectionElementKeyType = TargetCollectionElementType.GetGenericArguments()[ 0 ];
            var targetCollectionElementValueType = TargetCollectionElementType.GetGenericArguments()[ 1 ];

            SourceCollectionElementKey = Expression.Property( SourceCollectionLoopingVar, nameof( DictionaryEntry.Key ) );
            SourceCollectionElementValue = Expression.Property( SourceCollectionLoopingVar, nameof( DictionaryEntry.Value ) );

            TargetCollectionElementKey = Expression.Variable( targetCollectionElementKeyType, "targetKey" );
            TargetCollectionElementValue = Expression.Variable( targetCollectionElementValueType, "targetValue" );
        }
    }
}

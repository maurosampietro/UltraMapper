using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class DictionaryMapperContext : CollectionMapperContext
    {
        public MemberExpression SourceKey { get; private set; }
        public MemberExpression SourceValue { get; private set; }
        public Type SourceKeyType { get; private set; }
        public Type SourceValueType { get; private set; }
        public Type TargetKeyType { get; private set; }
        public Type TargetValueType { get; private set; }
        public ParameterExpression TargetKey { get; private set; }
        public ParameterExpression TargetValue { get; private set; }

        public DictionaryMapperContext( MemberMapping mapping )
            : base( mapping )
        {
            SourceKeyType = SourceElementType.GetGenericArguments()[ 0 ];
            SourceValueType = SourceElementType.GetGenericArguments()[ 1 ];

            TargetKeyType = TargetElementType.GetGenericArguments()[ 0 ];
            TargetValueType = TargetElementType.GetGenericArguments()[ 1 ];

            SourceKey = Expression.Property( SourceLoopingVar, nameof( DictionaryEntry.Key ) );
            SourceValue = Expression.Property( SourceLoopingVar, nameof( DictionaryEntry.Value ) );

            TargetKey = Expression.Variable( TargetKeyType, "targetKey" );
            TargetValue = Expression.Variable( TargetValueType, "targetValue" );
        }
    }
}

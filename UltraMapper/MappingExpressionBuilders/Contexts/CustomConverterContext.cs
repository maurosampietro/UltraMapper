using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace UltraMapper.MappingExpressionBuilders
{
    public class CustomConverterContext
    {
        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ParameterExpression ReferenceTracker { get; protected set; }
        public ParameterExpression TrackedReference { get; protected set; }

        public CustomConverterContext( Type source, Type target )
        {
            SourceInstance = Expression.Parameter( source, "sourceInstance" );
            TargetInstance = Expression.Parameter( target, "targetInstance" );
            ReferenceTracker = Expression.Parameter( typeof( ReferenceTracker ), "referenceTracker" );
            TrackedReference = Expression.Parameter( target, "trackedReference" );

            if( !SourceInstance.Type.IsValueType )
                SourceNullValue = Expression.Constant( null, SourceInstance.Type );

            if( !TargetInstance.Type.IsValueType )
                TargetNullValue = Expression.Constant( null, TargetInstance.Type );
        }
    }
}

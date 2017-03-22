using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class ReferenceMapperContext : MapperContext
    {
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }

        public ParameterExpression ReturnObject { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ParameterExpression ReferenceTrack { get; protected set; }

        public ReferenceMapperContext( Type source, Type target )
             : base( source, target ) { Initialize(); }

        private void Initialize()
        {
            var returnType = typeof( ObjectPair );
            ReturnTypeConstructor = returnType.GetConstructors().First();
            ReturnObject = Expression.Variable( returnType, "returnObject" );

            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            if( !SourceInstance.Type.IsValueType )
                SourceNullValue = Expression.Constant( null, SourceInstance.Type );

            if( !TargetInstance.Type.IsValueType )
                TargetNullValue = Expression.Constant( null, TargetInstance.Type );
        }
    }

    public class ReferenceMapperWithMemberMappingContext : MapperContext
    {
        public ParameterExpression ReferenceTrack { get; protected set; }

        public Type ReturnElementType { get; private set; }
        public ConstructorInfo ReturnTypeConstructor { get; private set; }
        public ParameterExpression ReturnObject { get; private set; }
        public Expression TargetNullValue { get; internal set; }

        public ReferenceMapperWithMemberMappingContext( TypeMapping mapping )
            : base( mapping.TypePair.SourceType, mapping.TypePair.TargetType ) { Initialize(); }

        public ReferenceMapperWithMemberMappingContext( Type source, Type target )
             : base( source, target ) { Initialize(); }

        private void Initialize()
        {
            var returnType = typeof( List<ObjectPair> );
            ReturnElementType = typeof( ObjectPair );

            ReturnTypeConstructor = returnType.GetConstructors().First();
            ReturnObject = Expression.Variable( returnType, "returnObject" );

            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            if( !TargetInstance.Type.IsValueType )
                TargetNullValue = Expression.Constant( null, TargetInstance.Type );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Internals;
using TypeMapper.Mappers.MapperContexts;

namespace TypeMapper.Mappers
{
    public class ReferenceMapperContext : MemberMappingContext
    {
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }

        public ParameterExpression ReturnObject { get; protected set; }

        public ConstantExpression SourceMemberNullValue { get; protected set; }
        public ConstantExpression TargetMemberNullValue { get; protected set; }

        public ReferenceMapperContext( MemberMapping mapping )
            : base( mapping ) { Initialize(); }

        public ReferenceMapperContext( TypeMapping mapping )
            : base( mapping ) { Initialize(); }

        public ReferenceMapperContext( Type source, Type target )
             : base( source, target ) { Initialize(); }

        private void Initialize()
        {
            var returnType = typeof( ObjectPair );
            ReturnTypeConstructor = returnType.GetConstructors().First();
            ReturnObject = Expression.Variable( returnType, "returnObject" );

            SourceMemberNullValue = Expression.Constant( null, SourceMember.Type );
            TargetMemberNullValue = Expression.Constant( null, TargetMember.Type );
        }
    }

    public class ReferenceMapperWithMemberMappingContext : MemberMappingContext
    {
        public Type ReturnElementType { get; private set; }
        public ConstructorInfo ReturnTypeConstructor { get; private set; }
        public ParameterExpression ReturnObject { get; private set; }

        public ReferenceMapperWithMemberMappingContext( MemberMapping mapping )
            : base( mapping ) { Initialize(); }

        public ReferenceMapperWithMemberMappingContext( TypeMapping mapping )
            : base( mapping ) { Initialize(); }

        public ReferenceMapperWithMemberMappingContext( Type source, Type target )
             : base( source, target ) { Initialize(); }

        private void Initialize()
        {
            var returnType = typeof( List<ObjectPair> );
            ReturnElementType = typeof( ObjectPair );

            ReturnTypeConstructor = returnType.GetConstructors().First();
            ReturnObject = Expression.Variable( returnType, "returnObject" );
        }
    }
}

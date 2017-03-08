using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapperContext : MapperContext
    {
        public Type ReturnType { get; protected set; }
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }

        public ParameterExpression ReturnObject { get; protected set; }

        public ConstantExpression SourceMemberNullValue { get; protected set; }
        public ConstantExpression TargetMemberNullValue { get; protected set; }

        public ReferenceMapperContext( MemberMapping mapping )
            : base( mapping ) { Initialize(); }

        public ReferenceMapperContext( TypeMapping mapping )
            : base( mapping ) { Initialize(); }

        private void Initialize()
        {
            ReturnType = typeof( ObjectPair );
            ReturnTypeConstructor = ReturnType.GetConstructors().First();

            ReturnObject = Expression.Variable( ReturnType, "result" );

            SourceMemberNullValue = Expression.Constant( null, SourceMemberType );
            TargetMemberNullValue = Expression.Constant( null, TargetMemberType );
        }
    }
}

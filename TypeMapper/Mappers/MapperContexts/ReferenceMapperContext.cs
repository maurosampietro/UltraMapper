using System;
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
            ReturnObject = Expression.Variable( returnType, "result" );

            SourceMemberNullValue = Expression.Constant( null, SourceMember.Type );
            TargetMemberNullValue = Expression.Constant( null, TargetMember.Type );
        }
    }
}

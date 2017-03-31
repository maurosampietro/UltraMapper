using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;
using UltraMapper.Mappers.MapperContexts;

namespace UltraMapper.Mappers
{
    public class ReferenceMapperContext : MapperContext
    {
        public Type ReturnElementType { get; protected set; }

        public ConstructorInfo ReturnTypeConstructor { get; protected set; }
        public MethodInfo AddObjectPairToReturnList { get; protected set; }

        public ParameterExpression ReturnObject { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ParameterExpression ReferenceTracker { get; protected set; }
        public ConstructorInfo ReturnElementConstructor { get; protected set; }

        public MethodInfo RecursiveMapMethodInfo { get; protected set; }
        public ParameterExpression Mapper { get; private set; }

        public ReferenceMapperContext( Type source, Type target )
             : base( source, target )
        {
            ReturnElementType = typeof( ObjectPair );
            var returnType = typeof( List<> ).MakeGenericType( ReturnElementType );

            ReturnObject = Expression.Variable( returnType, "returnObject" );
            ReturnTypeConstructor = returnType.GetConstructors().First();
            ReturnElementConstructor = ReturnElementType.GetConstructors()[ 0 ];

            var methodParams = new[] { ReturnElementType };
            AddObjectPairToReturnList = returnType.GetMethod( nameof( List<ObjectPair>.Add ), methodParams );

            ReferenceTracker = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            if( !SourceInstance.Type.IsValueType )
                SourceNullValue = Expression.Constant( null, SourceInstance.Type );

            if( !TargetInstance.Type.IsValueType )
                TargetNullValue = Expression.Constant( null, TargetInstance.Type );

            RecursiveMapMethodInfo = GetUltraMapperMapGenericMethod();
            Mapper = Expression.Variable( typeof( UltraMapper ), "mapper" );
        }

        private static MethodInfo GetUltraMapperMapGenericMethod()
        {
            return typeof( UltraMapper ).GetMethods()
                .Where( m => m.Name == "Map" )
                .Select( m => new
                {
                    Method = m,
                    Params = m.GetParameters(),
                    GenericArgs = m.GetGenericArguments()
                } )
                .Where( x => x.Params.Length == 3
                            && x.GenericArgs.Length == 2
                            && x.Params[ 0 ].ParameterType == x.GenericArgs[ 0 ] &&
                             x.Params[ 1 ].ParameterType == x.GenericArgs[ 1 ] &&
                             x.Params[ 2 ].ParameterType == typeof( ReferenceTracking ) )
                .Select( x => x.Method )
                .First();
        }

    }
}

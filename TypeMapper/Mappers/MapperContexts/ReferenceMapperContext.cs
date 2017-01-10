using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class ReferenceMapperContext
    {
        public readonly PropertyMapping Mapping;

        public Type ReturnType { get; protected set; }
        public ConstructorInfo ReturnTypeConstructor { get; protected set; }

        public Type SourceType { get; protected set; }
        public Type TargetType { get; protected set; }
        public Type SourcePropertyType { get; protected set; }
        public Type TargetPropertyType { get; protected set; }

        public ParameterExpression SourceInstance { get; protected set; }
        public ParameterExpression TargetInstance { get; protected set; }
        public ParameterExpression ReferenceTrack { get; protected set; }

        public ParameterExpression TargetPropertyVar { get; protected set; }
        public ParameterExpression SourcePropertyVar { get; protected set; }
        public ParameterExpression ReturnObjectVar { get; protected set; }

        public ConstantExpression SourceNullValue { get; protected set; }
        public ConstantExpression TargetNullValue { get; protected set; }

        public ReferenceMapperContext( PropertyMapping mapping )
        {
            Mapping = mapping;

            ReturnType = typeof( ObjectPair );
            ReturnTypeConstructor = ReturnType.GetConstructors().First();

            SourceType = mapping.SourceProperty.PropertyInfo.ReflectedType;
            TargetType = mapping.TargetProperty.PropertyInfo.ReflectedType;

            SourcePropertyType = mapping.SourceProperty.PropertyInfo.PropertyType;
            TargetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            SourceInstance = Expression.Parameter( SourceType, "sourceInstance" );
            TargetInstance = Expression.Parameter( TargetType, "targetInstance" );
            ReferenceTrack = Expression.Parameter( typeof( ReferenceTracking ), "referenceTracker" );

            SourcePropertyVar = Expression.Variable( SourcePropertyType, "sourceArg" );
            TargetPropertyVar = Expression.Variable( TargetPropertyType, "targetArg" );
            ReturnObjectVar = Expression.Variable( ReturnType, "result" );

            SourceNullValue = Expression.Constant( null, SourcePropertyType );
            TargetNullValue = Expression.Constant( null, TargetPropertyType );
        }
    }
}

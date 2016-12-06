using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Mappers;

namespace TypeMapper.Internals
{
    public class PropertyMapping
    {
        public SourceProperty SourceProperty { get; private set; }
        public TargetProperty TargetProperty { get; set; }
        public IObjectMapper Mapper { get; set; }
        public LambdaExpression ValueConverterExp { get; set; }

        private LambdaExpression _expression;
        public LambdaExpression Expression
        {
            get
            {
                if( _expression == null )
                {
                    if( Mapper.GetType() == typeof( ReferenceMapper ) )
                        _expression = this.GetExpression2();
                    else
                        _expression = this.GetExpression();
                }

                return _expression;
            }
        }

        public PropertyMapping( SourceProperty sourceProperty,
            TargetProperty targetProperty = null,
            LambdaExpression converterExp = null )
        {
            this.SourceProperty = sourceProperty;
            this.TargetProperty = targetProperty;
            this.ValueConverterExp = converterExp;
        }

        public override string ToString()
        {
            return $"{this.SourceProperty} -> {this.TargetProperty}";
        }
    }

    public static class PropertyMappingExtensions
    {
        public static LambdaExpression GetExpression( this PropertyMapping mapping )
        {
            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );

            var getValueExp = Expression.Invoke( mapping.SourceProperty.ValueGetterExpr, sourceInstance );

            var valueExp = getValueExp;
            if( mapping.ValueConverterExp != null )
                valueExp = Expression.Invoke( mapping.ValueConverterExp, getValueExp );

            var setValueExp = Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance,
                Expression.Convert( valueExp, targetPropertyType ) );

            var delegateType = typeof( Action<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType,
                setValueExp, sourceInstance, targetInstance );
        }

        public static LambdaExpression GetExpression2( this PropertyMapping mapping )
        {
            var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
            var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

            var targetPropertyType = mapping.TargetProperty.PropertyInfo.PropertyType;

            var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
            var targetInstance = Expression.Parameter( targetType, "targetInstance" );

            var getValueExp = Expression.Invoke( mapping.SourceProperty.ValueGetterExpr, sourceInstance );

            var nullExp = Expression.Constant( null );
            var conditionalExp =
               Expression.IfThenElse(
               Expression.Equal( getValueExp, nullExp ),
               Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance, Expression.Convert( nullExp, targetPropertyType ) ),
               Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance, Expression.New( targetPropertyType ) ) );

            var delegateType = typeof( Action<,> )
                .MakeGenericType( sourceType, targetType );

            return Expression.Lambda( delegateType,
                conditionalExp, sourceInstance, targetInstance );
        }
    }
}

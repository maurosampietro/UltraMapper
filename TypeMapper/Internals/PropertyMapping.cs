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

        //Generalize to Func<object,object> to avoid carrying too many generic T types around
        //and using Delegate and DynamicInvoke.
        public Func<object, object> ValueConverter { get; set; }

        public LambdaExpression ValueConverterExp { get; set; }

        private LambdaExpression _expression;
        public LambdaExpression Expression
        {
            get
            {
                if( _expression == null )
                    _expression = this.GetExpression();

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
            try
            {
                var sourceType = mapping.SourceProperty.PropertyInfo.DeclaringType;
                var targetType = mapping.TargetProperty.PropertyInfo.DeclaringType;

                var sourceInstance = Expression.Parameter( sourceType, "sourceInstance" );
                var targetInstance = Expression.Parameter( targetType, "targetInstance" );

                var getValueExp = Expression.Invoke( mapping.SourceProperty.ValueGetterExpr, sourceInstance );

                var valueExp = getValueExp;
                if( mapping.ValueConverterExp != null )
                    valueExp = Expression.Invoke( mapping.ValueConverterExp, getValueExp );

                var setValueExp = Expression.Invoke( mapping.TargetProperty.ValueSetterExpr, targetInstance,
                    Expression.Convert( valueExp, mapping.TargetProperty.
                    PropertyInfo.PropertyType ) );

                var delegateType = typeof( Action<,> )
                    .MakeGenericType( sourceType, targetType );

                return Expression.Lambda( delegateType,
                    setValueExp, sourceInstance, targetInstance );
            }
            catch( Exception )
            {

                return null;
            }
        }
    }
}

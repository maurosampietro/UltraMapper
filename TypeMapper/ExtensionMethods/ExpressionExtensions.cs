using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    internal static class ExpressionExtensions
    {
        public static PropertyInfo ExtractPropertyInfo( this Expression method )
        {
            var lambda = method as LambdaExpression;
            if( lambda == null )
                throw new InvalidCastException( "Invalid lambda expression" );

            /*
             For value-types automatic boxing operations are inserted as Convert(x).
             Multiple consecutive calls are possible.            
             */

            MemberExpression memberExpression = null;
            Expression expBody = lambda.Body;

            while( (memberExpression = expBody as MemberExpression) == null )
            {
                switch( expBody.NodeType )
                {
                    case ExpressionType.Convert:
                        {
                            expBody = ((UnaryExpression)expBody).Operand as Expression;
                            break;
                        }
                }
            }

            if( memberExpression == null )
                throw new ArgumentException( "Invalid expression. Please select a property from your model (eg. x => x.MyProperty)" );

            return (PropertyInfo)memberExpression.Member;
        }

        public static Expression<Func<object, object>> EncapsulateInGenericFunc<T>( this Expression expression )
        {
            return expression.EncapsulateInGenericFunc( typeof( T ) );
        }

        public static Expression<Func<object, object>> EncapsulateInGenericFunc( this Expression expression, Type convertType )
        {
            // o => Convert( Invoke( expression, Convert( o ) ) )

            var lambdaExpression = expression as LambdaExpression;

            var parameter = Expression.Parameter( typeof( object ), "o" );
            var convert = Expression.Convert( parameter, convertType );
            var encapsulatedExpression = Expression.Convert(
                Expression.Invoke( lambdaExpression, convert ), typeof( object ) );

            return Expression.Lambda<Func<object, object>>( encapsulatedExpression, parameter );
        }

        public static Expression<Func<T, V>> ToExpression<T, V>( this PropertyInfo property )
        {
            var parameter = Expression.Parameter( typeof( T ), "item" );
            var propertyBody = Expression.Property( parameter, property );
            var convert = Expression.Convert( propertyBody, typeof( V ) );

            return Expression.Lambda<Func<T, V>>( convert, parameter );
        }

        public static Expression<Func<T, IComparable>> ToExpression<T>( this PropertyInfo property )
        {
            var parameter = Expression.Parameter( typeof( T ), "item" );
            var propertyBody = Expression.Property( parameter, property );

            ////if not a valuetype check if null
            //if( !property.PropertyType.IsValueType )
            //{
            //    var nullTest = Expression.Equal( propertyBody,
            //        Expression.Constant( null, property.PropertyType ) );

            //    var isNullExp = Expression.Constant( String.Empty );
            //    //var notNullExp = Expression.Call( propertyBody, "ToString", null );

            //    //var cast = Expression.Condition( nullTest, isNullExp, notNullExp );
            //    var exp = Expression.Lambda<Func<T, IComparable>>( propertyBody, parameter );

            //    return exp;
            //}

            var convert = Expression.Convert( propertyBody, typeof( IComparable ) );
            return Expression.Lambda<Func<T, IComparable>>( convert, parameter );
        }

        public static Expression<Func<T, ReturnType>> ConvertReturnType<T, V, ReturnType>( this Expression<Func<T, V>> expression )
        {
            var convert = Expression.Convert( expression.Body, typeof( ReturnType ) );
            return Expression.Lambda<Func<T, ReturnType>>( convert, expression.Parameters );
        }

        //public static T GetDefaultValue<T>()
        //{
        //    var e = Expression.Lambda<Func<T>>( Expression.Default( typeof( T ) ) );

        //    // Compile and return the value.
        //    return e.Compile()();
        //}

        public static object GetDefaultValue( this Type type )
        {
            if( type == null ) throw new ArgumentNullException( nameof( type ) );

            var expression = Expression.Lambda<Func<object>>(
                Expression.Convert( Expression.Default( type ), typeof( object ) ) );

            return expression.Compile()();
        }
    }
}

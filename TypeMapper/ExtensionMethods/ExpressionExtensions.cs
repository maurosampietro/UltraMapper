using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TypeMapper.Internals;

namespace TypeMapper
{
    internal static class ExpressionExtensions
    {
        private static string invalidExpressionMsg =
            "Invalid expression. Please select a property from your model (eg. x => x.MyProperty)";

        public static MemberInfo ExtractMember( this Expression method )
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
                if( expBody.NodeType == ExpressionType.Convert )
                    expBody = ((UnaryExpression)expBody).Operand as Expression;

                else if( expBody.NodeType == ExpressionType.Call )
                    return ((MethodCallExpression)expBody).Method;
            }

            if( memberExpression == null )
                throw new ArgumentException( invalidExpressionMsg );

            //If the instance on which we call is a derived class and the property
            //we select is defined in the base class, we will notice that
            //the PropertyInfo is retrieved through the base class; hence
            //DeclaredType and ReflectedType are equal and we basically
            //lose information about the ReflectedType (which should be the derived class)...
            var lambdaMember = memberExpression.Member;

            try
            {
                //..to fix that we do another search. 
                //We search that property name in the actual type we meant to use for the invocation
                return lambda.Parameters.First().Type.GetMember( lambdaMember.Name )[ 0 ];
            }
            catch( Exception ex )
            {
                //if the property we searched for do not exists we probably are 
                //selecting a nested property, so just return that
                return lambdaMember;
            }
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

        public static object GetDefaultValueViaExpressionCompilation( this Type type )
        {
            if( type == null ) throw new ArgumentNullException( nameof( type ) );

            var expression = Expression.Lambda<Func<object>>(
                Expression.Convert( Expression.Default( type ), typeof( object ) ) );

            return expression.Compile()();
        }

        public static Expression ReplaceParameter( this Expression expression, ParameterExpression parameter )
        {
            return new ExpressionParameterReplacer( parameter ).Visit( expression );
        }

        public static Expression ReplaceParameter( this Expression expression, ParameterExpression parameter, string name )
        {
            return new ExpressionParameterReplacer( parameter, name ).Visit( expression );
        }
    }

    internal static class ExpressionLoops
    {
        public static Expression ForEach( Expression collection,
            ParameterExpression loopVar, Expression loopContent )
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof( IEnumerable<> ).MakeGenericType( elementType );
            var enumeratorType = typeof( IEnumerator<> ).MakeGenericType( elementType );

            var enumeratorVar = Expression.Variable( enumeratorType, "enumerator" );
            var getEnumeratorCall = Expression.Call( collection, enumerableType.GetMethod( "GetEnumerator" ) );
            var enumeratorAssign = Expression.Assign( enumeratorVar, getEnumeratorCall );

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call( enumeratorVar, typeof( IEnumerator ).GetMethod( "MoveNext" ) );
            var breakLabel = Expression.Label( "LoopBreak" );

            var loop = Expression.Block
            (
                new[] { enumeratorVar },

                enumeratorAssign,

                Expression.Loop
                (
                    Expression.IfThenElse
                    (
                        Expression.Equal( moveNextCall, Expression.Constant( true ) ),
                        Expression.Block
                        (
                            new[] { loopVar },

                            Expression.Assign( loopVar, Expression.Property( enumeratorVar, "Current" ) ),
                            loopContent
                        ),

                        Expression.Break( breakLabel )
                    ),

                    breakLabel
               )
            );

            return loop;
        }
    }
}

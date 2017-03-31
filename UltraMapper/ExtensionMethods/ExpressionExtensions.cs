﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper
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

                else if( expBody.NodeType == ExpressionType.Parameter )
                    return ((ParameterExpression)expBody).Type;
            }

            if( memberExpression == null )
                throw new ArgumentException( invalidExpressionMsg );

            //If the instance on which we call is a derived class and the property
            //we select is defined in the base class, we will notice that
            //the PropertyInfo is retrieved through the base class; hence
            //DeclaredType and ReflectedType are equal and we basically
            //lose information about the ReflectedType (which should be the derived class)...
            //..to fix that we do another search. 
            //We search the member we are accessing by name in the actual type we meant to use for the invocation
            //Since we support deep member accessing, things get a little more complex here
            //but we basically just follow each member access starting from the passed lambda parameter.

            //Reverse the expression accessing order to make it easy to work with it
            Stack<Expression> stack = GetNaturalExpressionAccessOrder( memberExpression );

            //Follow member accesses starting from the lambda input parameter.
            var lambdaMember = (stack.Pop() as ParameterExpression);
            var member = lambda.Parameters.First(
                p => p.Name == lambdaMember.Name ).Type as MemberInfo;

            foreach( var item in stack )
            {
                string memberName = null;

                if( item is MemberExpression )
                    memberName = ((MemberExpression)item).Member.Name;
                else if( item is MethodCallExpression )
                    memberName = ((MethodCallExpression)item).Method.Name;

                member = member.GetMemberType().GetMember( memberName )[ 0 ];
            }

            return member;
        }

        private static Stack<Expression> GetNaturalExpressionAccessOrder( Expression expression )
        {
            Stack<Expression> stack = new Stack<Expression>();

            while( !(expression is ParameterExpression) )
            {
                stack.Push( expression );

                if( expression is MemberExpression )
                    expression = ((MemberExpression)expression).Expression;
                else if( expression is MethodCallExpression )
                    expression = ((MethodCallExpression)expression).Object;
            }

            stack.Push( expression );
            return stack;
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
            var moveNextCall = Expression.Call( enumeratorVar, typeof( IEnumerator )
                .GetMethod( nameof( IEnumerator.MoveNext ) ) );

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
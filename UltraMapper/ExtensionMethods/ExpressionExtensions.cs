using System;
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

        public static Expression ReplaceParameter( this Expression expression, ParameterExpression parameter, string name )
        {
            return new ExpressionParameterReplacer( parameter, name ).Visit( expression );
        }
    }

    internal static class ExpressionLoops
    {
        public static Expression ForEach( Expression collection, ParameterExpression loopVar, Expression loopContent )
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

    internal static class GetterSetterExpressionBuilder
    {
        public static LambdaExpression GetGetterLambdaExpression( this MemberInfo memberInfo )
        {
            if( memberInfo is FieldInfo )
                return GetGetterLambdaExpression( (FieldInfo)memberInfo );

            if( memberInfo is PropertyInfo )
                return GetGetterLambdaExpression( (PropertyInfo)memberInfo );

            if( memberInfo is MethodInfo )
                return GetGetterLambdaExpression( (MethodInfo)memberInfo );

            throw new ArgumentException( $"'{memberInfo}' is not supported." );
        }

        public static LambdaExpression GetSetterLambdaExpression( this MemberInfo memberInfo )
        {
            if( memberInfo is Type )
            {
                var type = (Type)memberInfo;
                // (target, value) => target.field;

                var targetInstance = Expression.Parameter( type, "target" );
                var value = Expression.Parameter( type, "value" );

                var body = Expression.Assign( targetInstance, value );

                var delegateType = typeof( Action<,> ).MakeGenericType( type, type );

                return LambdaExpression.Lambda( delegateType, body, targetInstance, value );
            }

            if( memberInfo is FieldInfo )
                return GetSetterLambdaExpression( (FieldInfo)memberInfo );

            if( memberInfo is PropertyInfo )
                return GetSetterLambdaExpression( (PropertyInfo)memberInfo );

            if( memberInfo is MethodInfo )
                return GetSetterLambdaExpression( (MethodInfo)memberInfo );

            throw new ArgumentException( $"'{memberInfo}' is not supported." );
        }

        public static LambdaExpression GetGetterLambdaExpression( this FieldInfo fieldInfo )
        {
            // (target) => target.field;

            var targetInstance = Expression.Parameter( fieldInfo.ReflectedType, "target" );
            var body = Expression.Field( targetInstance, fieldInfo );

            var delegateType = typeof( Func<,> ).MakeGenericType(
                fieldInfo.ReflectedType, fieldInfo.FieldType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance );
        }

        public static LambdaExpression GetSetterLambdaExpression( this FieldInfo fieldInfo )
        {
            // (target, value) => target.field = value;

            var targetInstance = Expression.Parameter( fieldInfo.ReflectedType, "target" );
            var value = Expression.Parameter( fieldInfo.FieldType, "value" );

            var fieldExp = Expression.Field( targetInstance, fieldInfo );
            var body = Expression.Assign( fieldExp, value );

            var delegateType = typeof( Action<,> ).MakeGenericType(
                fieldInfo.ReflectedType, fieldInfo.FieldType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance, value );
        }

        public static LambdaExpression GetGetterLambdaExpression( this PropertyInfo propertyInfo )
        {
            // (target) => target.get_Property()
            var targetType = propertyInfo.ReflectedType;
            var methodInfo = propertyInfo.GetGetMethod();

            var targetInstance = Expression.Parameter( targetType, "target" );
            var body = Expression.Call( targetInstance, methodInfo );

            var delegateType = typeof( Func<,> ).MakeGenericType(
                propertyInfo.ReflectedType, propertyInfo.PropertyType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance );
        }

        public static LambdaExpression GetSetterLambdaExpression( this PropertyInfo propertyInfo )
        {
            // (target, value) => target.set_Property( value )
            var methodInfo = propertyInfo.GetSetMethod();
            if( methodInfo == null )
                throw new ArgumentException( $"'{propertyInfo}' does not provide a setter method." );

            var targetInstance = Expression.Parameter( propertyInfo.ReflectedType, "target" );
            var value = Expression.Parameter( propertyInfo.PropertyType, "value" );

            var body = Expression.Call( targetInstance, methodInfo, value );

            var delegateType = typeof( Action<,> ).MakeGenericType(
                propertyInfo.ReflectedType, propertyInfo.PropertyType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance, value );
        }

        public static LambdaExpression GetGetterLambdaExpression( this MethodInfo methodInfo )
        {
            if( methodInfo.GetParameters().Length > 0 )
                throw new NotImplementedException( "Only parameterless methods are currently supported" );

            var targetType = methodInfo.ReflectedType;

            var targetInstance = Expression.Parameter( targetType, "target" );
            var body = Expression.Call( targetInstance, methodInfo );

            var delegateType = typeof( Func<,> ).MakeGenericType(
                methodInfo.ReflectedType, methodInfo.ReturnType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance );
        }

        public static LambdaExpression GetSetterLambdaExpression( this MethodInfo methodInfo )
        {
            if( methodInfo.GetParameters().Length != 1 )
                throw new NotImplementedException( $"Only methods taking as input exactly one parameter are currently supported." );

            var targetInstance = Expression.Parameter( methodInfo.ReflectedType, "target" );
            var value = Expression.Parameter( methodInfo.GetParameters()[ 0 ].ParameterType, "value" );

            var body = Expression.Call( targetInstance, methodInfo, value );

            var delegateType = typeof( Action<,> ).MakeGenericType(
                methodInfo.ReflectedType, methodInfo.GetParameters()[ 0 ].ParameterType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance, value );
        }
    }
}

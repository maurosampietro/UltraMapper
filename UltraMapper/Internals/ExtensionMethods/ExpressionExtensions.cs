using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    internal static class ExpressionExtensions
    {
        public static MemberAccessPath ExtractMember( this Expression lambdaExpression )
        {
            var memberAcessPath = new MemberAccessPath();

            var lambda = lambdaExpression as LambdaExpression;
            if( lambda == null )
                throw new InvalidCastException( "Invalid lambda expression" );

            var stack = new Stack<Expression>();
            Expression exp = lambda.Body;

            //we are always only interested in the left part of an assignment expression.
            if( exp.NodeType == ExpressionType.Assign )
                exp = ((BinaryExpression)exp).Left;

            //if the expression is a constant, we just return the type of the constant
            else if( exp.NodeType == ExpressionType.Constant )
            {
                memberAcessPath.Add( ((ConstantExpression)exp).Type );
                return memberAcessPath;
            }

            //break the expression down member by member
            while( !(exp is ParameterExpression) )
            {
                stack.Push( exp );

                if( exp.NodeType == ExpressionType.Convert )
                    exp = ((UnaryExpression)exp).Operand as Expression;

                else if( exp.NodeType == ExpressionType.MemberAccess )
                    exp = ((MemberExpression)exp).Expression;

                else if( exp.NodeType == ExpressionType.Call )
                    exp = ((MethodCallExpression)exp).Object;
            }

            //instance parameter
            stack.Push( exp );


            //If the instance on which we call is a derived class and the property
            //we select is defined in the base class, we will notice that
            //the PropertyInfo is retrieved through the base class; hence
            //DeclaredType and ReflectedType are equal and we basically
            //lose information about the ReflectedType (which should be the derived class)...
            //..to fix that we do another search. 
            //We search the member we are accessing by name in the actual type we meant to use for the invocation
            //Since we support deep member accessing, things get a little more complex here
            //but we basically just follow each member access starting from the passed lambda parameter.

            memberAcessPath = new MemberAccessPath();

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

                member = member.GetMemberType().GetMember( memberName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )[ 0 ];

                memberAcessPath.Add( member );
            }

            return memberAcessPath;
        }

        public static Expression ReplaceParameter( this Expression expression, Expression newExpression, string name )
        {
            return new ExpressionParameterReplacer( newExpression, name ).Visit( expression );
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

            //The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call( enumeratorVar, typeof( IEnumerator )
                .GetMethod( nameof( IEnumerator.MoveNext ) ) );

            var breakLabel = Expression.Label( "LoopBreak" );

            return Expression.Block
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
        }

        //public static Expression For( ParameterExpression loopVar, Expression initValue,
        //    Expression condition, Expression increment, Expression loopContent )
        //{
        //    var initAssign = Expression.Assign( loopVar, initValue );

        //    var breakLabel = Expression.Label( "LoopBreak" );

        //    var loop = Expression.Block( new[] { loopVar },
        //        initAssign,
        //        Expression.Loop(
        //            Expression.IfThenElse(
        //                condition,
        //                Expression.Block(
        //                    loopContent,
        //                    increment
        //                ),
        //                Expression.Break( breakLabel )
        //            ),
        //        breakLabel )
        //    );

        //    return loop;
        //}
    }

    //Generating getter/setter expression from MemberInfo does not preserve the entry instance type
    //if the member is extracted from a complex expression chain 
    //(for example in the expression 'a => a.PropertyA.PropertyB.PropertyC'
    //the ReflectedType info (that should be of type 'a') is lost in PropertyC (will be type of 'PropertyC'))
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
            var methodInfo = propertyInfo.GetGetMethod( true );

            var targetInstance = Expression.Parameter( targetType, "target" );
            var body = Expression.Call( targetInstance, methodInfo );

            var delegateType = typeof( Func<,> ).MakeGenericType(
                targetType, propertyInfo.PropertyType );

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
                throw new NotImplementedException( "Only parameterless methods are supported" );

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
                throw new NotImplementedException( $"Only methods taking as input exactly one parameter are supported." );

            var targetInstance = Expression.Parameter( methodInfo.ReflectedType, "target" );
            var value = Expression.Parameter( methodInfo.GetParameters()[ 0 ].ParameterType, "value" );

            var body = Expression.Call( targetInstance, methodInfo, value );

            var delegateType = typeof( Action<,> ).MakeGenericType(
                methodInfo.ReflectedType, methodInfo.GetParameters()[ 0 ].ParameterType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance, value );
        }
    }

    //Generating getter/setter expression from MemberAccessPath guarantees that the entry instance type is preserved
    internal static class MemberAccessPathExpressionBuilder
    {
        internal static LambdaExpression GetGetterLambdaExpression( this MemberAccessPath memberAccessPath )
        {
            var instanceType = memberAccessPath.First().ReflectedType;
            var returnType = memberAccessPath.Last().GetMemberType();

            var accessInstance = Expression.Parameter( instanceType, "instance" );
            Expression accessPath = accessInstance;
            foreach( var memberAccess in memberAccessPath )
            {
                if( memberAccess is MethodInfo )
                    accessPath = Expression.Call( accessPath, (MethodInfo)memberAccess );
                else
                    accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );
            }

            var delegateType = typeof( Func<,> ).MakeGenericType( instanceType, returnType );
            return LambdaExpression.Lambda( delegateType, accessPath, accessInstance );
        }

        internal static LambdaExpression GetSetterLambdaExpression( this MemberAccessPath memberAccessPath )
        {
            var instanceType = memberAccessPath.First().ReflectedType;
            var valueType = memberAccessPath.Last().GetMemberType();

            var value = Expression.Parameter( valueType, "value" );
            var accessInstance = Expression.Parameter( instanceType, "instance" );

            Expression accessPath = accessInstance;

            foreach( var memberAccess in memberAccessPath )
            {
                if( memberAccess is MethodInfo )
                {
                    var methodInfo = (MethodInfo)memberAccess;

                    if( methodInfo.IsGetterMethod() )
                        accessPath = Expression.Call( accessPath, methodInfo );
                    else
                        accessPath = Expression.Call( accessPath, (MethodInfo)memberAccess, value );
                }
                else
                    accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );
            }

            if( !(accessPath is MethodCallExpression) )
                accessPath = Expression.Assign( accessPath, value );

            var delegateType = typeof( Action<,> ).MakeGenericType( instanceType, valueType );
            return LambdaExpression.Lambda( delegateType, accessPath, accessInstance, value );
        }

        internal static LambdaExpression GetGetterLambdaExpressionWithNullChecks( this MemberAccessPath memberAccessPath )
        {
            var instanceType = memberAccessPath.First().ReflectedType;
            var returnType = memberAccessPath.Last().GetMemberType();

            var entryInstance = Expression.Parameter( instanceType, "instance" );
            var labelTarget = Expression.Label( returnType, "label" );

            Expression accessPath = entryInstance;
            var memberAccesses = new List<Expression>();

            foreach( var memberAccess in memberAccessPath )
            {
                if( memberAccess is MethodInfo )
                    accessPath = Expression.Call( accessPath, (MethodInfo)memberAccess );
                else
                    accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );

                memberAccesses.Add( accessPath );
            }

            var nullConstant = Expression.Constant( null );
            var returnNull = Expression.Return( labelTarget, Expression.Constant( null, returnType ) );

            var nullChecks = memberAccesses.Take( memberAccesses.Count - 1 ).Select( memberAccess =>
            {
                var equalsNull = Expression.Equal( memberAccess, nullConstant );
                return (Expression)Expression.IfThen( equalsNull, returnNull );

            } ).ToList();

            var exp = Expression.Block
            (
                nullChecks.Any() ? Expression.Block( nullChecks.ToArray() )
                    : (Expression)Expression.Empty(),

                Expression.Label( labelTarget, memberAccesses.Last() )
            );

            var delegateType = typeof( Func<,> ).MakeGenericType( instanceType, returnType );
            return LambdaExpression.Lambda( delegateType, exp, entryInstance );
        }

        internal static LambdaExpression GetSetterLambdaExpressionWithNullChecks( this MemberAccessPath memberAccessPath )
        {
            var instanceType = memberAccessPath.First().ReflectedType;
            var valueType = memberAccessPath.Last().GetMemberType();
            var value = Expression.Parameter( valueType, "value" );

            var entryInstance = Expression.Parameter( instanceType, "instance" );
            var labelTarget = Expression.Label( typeof( void ), "label" );

            Expression accessPath = entryInstance;
            var memberAccesses = new List<Expression>();

            foreach( var memberAccess in memberAccessPath )
            {
                if( memberAccess is MethodInfo )
                {
                    var methodInfo = (MethodInfo)memberAccess;

                    if( methodInfo.IsGetterMethod() )
                        accessPath = Expression.Call( accessPath, methodInfo );
                    else
                        accessPath = Expression.Call( accessPath, methodInfo, value );
                }
                else
                    accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );

                memberAccesses.Add( accessPath );
            }

            if( !(accessPath is MethodCallExpression) )
                accessPath = Expression.Assign( accessPath, value );

            var nullConstant = Expression.Constant( null );
            var returnVoid = Expression.Return( labelTarget, typeof( void ) );

            var nullChecks = memberAccesses.Take( memberAccesses.Count - 1 ).Select( memberAccess =>
            {
                var equalsNull = Expression.Equal( memberAccess, nullConstant );
                return (Expression)Expression.IfThen( equalsNull, returnVoid );

            } ).ToList();

            var exp = Expression.Block
            (
                nullChecks.Any() ? Expression.Block( nullChecks.ToArray() )
                    : (Expression)Expression.Empty(),

                accessPath,               
                Expression.Label( labelTarget )
            );

            var delegateType = typeof( Action<,> ).MakeGenericType( instanceType, valueType );
            return LambdaExpression.Lambda( delegateType, exp, entryInstance, value );
        }
    }
}

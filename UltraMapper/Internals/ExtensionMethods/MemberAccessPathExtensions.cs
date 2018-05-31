using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
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
            var returnNull = Expression.Return( labelTarget, Expression.Default( returnType ) );

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

        internal static LambdaExpression GetSetterLambdaExpressionWithNullInstancesInstantiation( this MemberAccessPath memberAccessPath )
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
            var nullChecks = memberAccesses.Take( memberAccesses.Count - 1 ).Select( memberAccess =>
            {
                var createInstance = Expression.Assign( memberAccess, Expression.New( memberAccess.Type ) );
                var equalsNull = Expression.Equal( memberAccess, nullConstant );
                return (Expression)Expression.IfThen( equalsNull, createInstance );

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

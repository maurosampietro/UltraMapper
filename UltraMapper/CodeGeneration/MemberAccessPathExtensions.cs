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
            var entryMember = memberAccessPath.First();

            var instanceType = entryMember.ReflectedType ??
                entryMember.GetMemberType().UnderlyingSystemType;

            var returnType = memberAccessPath.Last().GetMemberType();

            var accessInstance = Expression.Parameter( instanceType, "instance" );
            Expression accessPath = accessInstance;

            if( memberAccessPath.Count == 1 && entryMember is Type )
            {
                //instance => instance
                //do nothing
            }
            else
            {
                foreach( var memberAccess in memberAccessPath )
                {
                    if( memberAccess is MethodInfo )
                        accessPath = Expression.Call( accessPath, (MethodInfo)memberAccess );
                    else
                        accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );
                }
            }

            var delegateType = typeof( Func<,> ).MakeGenericType( instanceType, returnType );
            return LambdaExpression.Lambda( delegateType, accessPath, accessInstance );
        }

        internal static LambdaExpression GetSetterLambdaExpression( this MemberAccessPath memberAccessPath )
        {
            var entryMember = memberAccessPath.First();

            var instanceType = entryMember.ReflectedType ??
                entryMember.GetMemberType().UnderlyingSystemType;

            var valueType = memberAccessPath.Last().GetMemberType();

            var value = Expression.Parameter( valueType, "value" );
            var accessInstance = Expression.Parameter( instanceType, "instance" );

            Expression accessPath = accessInstance;

            if( memberAccessPath.Count == 1 && entryMember is Type )
            {
                //instance => instance
                //do nothing
            }
            else
            {
                foreach( var memberAccess in memberAccessPath )
                {
                    if( memberAccess is MethodInfo methodInfo )
                    {
                        if( methodInfo.IsGetterMethod() )
                            accessPath = Expression.Call( accessPath, methodInfo );
                        else
                            accessPath = Expression.Call( accessPath, (MethodInfo)memberAccess, value );
                    }
                    else
                        accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );
                }
            }

            if( !(accessPath is MethodCallExpression) )
                accessPath = Expression.Assign( accessPath, value );

            var delegateType = typeof( Action<,> ).MakeGenericType( instanceType, valueType );
            return LambdaExpression.Lambda( delegateType, accessPath, accessInstance, value );
        }

        internal static LambdaExpression GetGetterLambdaExpressionWithNullChecks( this MemberAccessPath memberAccessPath )
        {
            var entryMember = memberAccessPath.First();

            var instanceType = entryMember.ReflectedType ??
                entryMember.GetMemberType().UnderlyingSystemType;

            var returnType = memberAccessPath.Last().GetMemberType();

            var entryInstance = Expression.Parameter( instanceType, "instance" );
            var labelTarget = Expression.Label( returnType, "label" );

            Expression accessPath = entryInstance;
            var memberAccesses = new List<Expression>();

            if( memberAccessPath.Count == 1 && entryMember is Type )
            {
                //instance => instance
                //do nothing
            }
            else
            {
                foreach( var memberAccess in memberAccessPath )
                {
                    if( memberAccess is MethodInfo )
                        accessPath = Expression.Call( accessPath, (MethodInfo)memberAccess );
                    else
                        accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );

                    memberAccesses.Add( accessPath );
                }
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
            var entryMember = memberAccessPath.First();

            var instanceType = entryMember.ReflectedType ??
                entryMember.GetMemberType().UnderlyingSystemType;

            var valueType = memberAccessPath.Last().GetMemberType();
            var value = Expression.Parameter( valueType, "value" );

            var entryInstance = Expression.Parameter( instanceType, "instance" );
            var labelTarget = Expression.Label( typeof( void ), "label" );

            Expression accessPath = entryInstance;
            var memberAccesses = new List<Expression>();

            if( memberAccessPath.Count == 1 && entryMember is Type )
            {
                //instance => instance
                //do nothing
            }
            else
            {
                foreach( var memberAccess in memberAccessPath )
                {
                    if( memberAccess is MethodInfo methodInfo )
                    {
                        if( methodInfo.IsGetterMethod() )
                            accessPath = Expression.Call( accessPath, methodInfo );
                        else
                            accessPath = Expression.Call( accessPath, methodInfo, value );
                    }
                    else
                        accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );

                    memberAccesses.Add( accessPath );
                }
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
            var entryMember = memberAccessPath.First();

            var instanceType = entryMember.ReflectedType ??
                entryMember.GetMemberType().UnderlyingSystemType;

            var valueType = memberAccessPath.Last().GetMemberType();
            var value = Expression.Parameter( valueType, "value" );

            var entryInstance = Expression.Parameter( instanceType, "instance" );
            var labelTarget = Expression.Label( typeof( void ), "label" );

            Expression accessPath = entryInstance;
            var memberAccesses = new List<Expression>();

            if( memberAccessPath.Count == 1 && entryMember is Type )
            {
                //instance => instance
                //do nothing
            }
            else
            {
                foreach( var memberAccess in memberAccessPath )
                {
                    if( memberAccess is MethodInfo methodInfo )
                    {
                        if( methodInfo.IsGetterMethod() )
                            accessPath = Expression.Call( accessPath, methodInfo );
                        else
                            accessPath = Expression.Call( accessPath, methodInfo, value );
                    }
                    else
                        accessPath = Expression.MakeMemberAccess( accessPath, memberAccess );

                    memberAccesses.Add( accessPath );
                }
            }

            if( !(accessPath is MethodCallExpression) )
                accessPath = Expression.Assign( accessPath, value );

            var nullConstant = Expression.Constant( null );
            var nullChecks = memberAccesses.Take( memberAccesses.Count - 1 ).Select( ( memberAccess, i ) =>
            {
                if( memberAccessPath[ i ] is MethodInfo methodInfo )
                {
                    //nested method calls like GetCustomer().SetName() include non-writable member (GetCustomer).
                    //Assigning a new instance in that case is more difficult.
                    //In that case 'by convention' we should look for:
                    // - A property named Customer
                    // - A method named SetCustomer(argument type = getter return type) 
                    //      (also take into account Set, Set_, set, set_) as for convention.

                    var bindingAttributes = BindingFlags.Instance | BindingFlags.Public
                        | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic;

                    string setterMethodName = null;
                    if( methodInfo.Name.StartsWith( "Get" ) )
                        setterMethodName = methodInfo.Name.Replace( "Get", "Set" );
                    else if( methodInfo.Name.StartsWith( "get" ) )
                        setterMethodName = methodInfo.Name.Replace( "get", "set" );
                    else if( methodInfo.Name.StartsWith( "Get_" ) )
                        setterMethodName = methodInfo.Name.Replace( "Get_", "Set_" );
                    else if( methodInfo.Name.StartsWith( "get_" ) )
                        setterMethodName = methodInfo.Name.Replace( "get_", "set_" );

                    var setterMethod = methodInfo.ReflectedType.GetMethod( setterMethodName, bindingAttributes );

                    Expression setterAccessPath = entryInstance;
                    for( int j = 0; j < i; j++ )
                    {
                        if( memberAccessPath[ j ] is MethodInfo mi )
                        {
                            if( mi.IsGetterMethod() )
                                setterAccessPath = Expression.Call( accessPath, mi );
                            else
                                setterAccessPath = Expression.Call( accessPath, mi, value );
                        }
                        else
                            setterAccessPath = Expression.MakeMemberAccess( setterAccessPath, memberAccessPath[ j ] );
                    }

                    setterAccessPath = Expression.Call( setterAccessPath, setterMethod, Expression.New( memberAccess.Type ) );
                    var equalsNull = Expression.Equal( memberAccess, nullConstant );
                    return (Expression)Expression.IfThen( equalsNull, setterAccessPath );
                }
                else
                {
                    var createInstance = Expression.Assign( memberAccess, Expression.New( memberAccess.Type ) );
                    var equalsNull = Expression.Equal( memberAccess, nullConstant );
                    return (Expression)Expression.IfThen( equalsNull, createInstance );
                }

            } ).Where( nc => nc != null ).ToList();

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

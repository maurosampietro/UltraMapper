using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    //Generating getter/setter expression from MemberInfo does not preserve the entry instance type
    //if the member is extracted from a complex expression chain 
    //(for example in the expression 'a => a.PropertyA.PropertyB.PropertyC'
    //the ReflectedType info of PropertyC (that should be of type 'a') is lost (and will be of the type of 'PropertyC'))
    internal static class GetterSetterExpressionBuilders
    {
        internal static LambdaExpression GetGetterExp( this MemberInfo memberInfo )
        {
            return memberInfo switch
            {
                Type type => type.GetGetterExp(),
                FieldInfo fi => fi.GetGetterExp(),
                PropertyInfo pi => pi.GetGetterExp(),
                MethodInfo mi => mi.GetGetterExp(),
                _ => throw new ArgumentException( $"'{memberInfo}' is not supported." ),
            };
        }

        internal static LambdaExpression GetGetterExp( this Type type )
        {
            LabelTarget returnTarget = Expression.Label( type );

            var targetInstance = Expression.Parameter( type, "target" );
            var body = Expression.Return( returnTarget, targetInstance, type );

            var delegateType = typeof( Func<,> ).MakeGenericType( type, type );
            return LambdaExpression.Lambda( delegateType, body, targetInstance );
        }

        internal static LambdaExpression GetSetterExp( this MemberInfo memberInfo )
        {
            switch( memberInfo )
            {
                case Type type:
                {
                    // (target, value) => target.field;

                    var targetInstance = Expression.Parameter( type, "target" );
                    var value = Expression.Parameter( type, "value" );

                    var body = Expression.Assign( targetInstance, value );
                    var delegateType = typeof( Action<,> ).MakeGenericType( type, type );

                    return Expression.Lambda( delegateType, body, targetInstance, value );
                }

                case FieldInfo fieldInfo:
                    return fieldInfo.GetSetterExp();

                case PropertyInfo propertyInfo:
                    return propertyInfo.GetSetterExp();

                case MethodInfo methodInfo:
                    return methodInfo.GetSetterExp();

                default:
                    throw new ArgumentException( $"'{memberInfo}' is not supported." );
            }
        }

        internal static LambdaExpression GetGetterExp( this FieldInfo fieldInfo )
        {
            // (target) => target.field;

            var targetInstance = Expression.Parameter( fieldInfo.ReflectedType, "target" );
            var body = Expression.Field( targetInstance, fieldInfo );

            var delegateType = typeof( Func<,> ).MakeGenericType(
                fieldInfo.ReflectedType, fieldInfo.FieldType );

            return LambdaExpression.Lambda( delegateType, body, targetInstance );
        }

        internal static LambdaExpression GetSetterExp( this FieldInfo fieldInfo )
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

        internal static LambdaExpression GetGetterExp( this PropertyInfo propertyInfo )
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

        internal static LambdaExpression GetSetterExp( this PropertyInfo propertyInfo )
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

        internal static LambdaExpression GetGetterExp( this MethodInfo methodInfo )
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

        internal static LambdaExpression GetSetterExp( this MethodInfo methodInfo )
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

        internal static LambdaExpression GetSetterExpInstantiateNullInstances( this MemberAccessPath memberAccessPath )
        {
            var instanceType = memberAccessPath.First().ReflectedType;
            var valueType = memberAccessPath.Last().GetMemberType();
            var value = Expression.Parameter( valueType, "value" );

            var entryInstance = Expression.Parameter( instanceType, "instance" );

            Expression accessPath = entryInstance;
            var memberAccesses = new List<Expression>();

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

            if( accessPath is not MethodCallExpression )
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

                accessPath
            );

            var delegateType = typeof( Action<,> ).MakeGenericType( instanceType, valueType );
            return LambdaExpression.Lambda( delegateType, exp, entryInstance, value );
        }
    }
}

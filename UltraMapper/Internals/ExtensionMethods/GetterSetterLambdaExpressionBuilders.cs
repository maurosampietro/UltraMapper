using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UltraMapper.Internals
{
    //Generating getter/setter expression from MemberInfo does not preserve the entry instance type
    //if the member is extracted from a complex expression chain 
    //(for example in the expression 'a => a.PropertyA.PropertyB.PropertyC'
    //the ReflectedType info of PropertyC (that should be of type 'a') is lost (and will be of the type of 'PropertyC'))
    internal static class GetterSetterLambdaExpressionBuilders
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
            if( memberInfo is Type type )
            {
                // (target, value) => target.field;

                var targetInstance = Expression.Parameter( type, "target" );
                var value = Expression.Parameter( type, "value" );

                var body = Expression.Assign( targetInstance, value );
                var delegateType = typeof( Action<,> ).MakeGenericType( type, type );

                return LambdaExpression.Lambda( delegateType, body, targetInstance, value );
            }

            if( memberInfo is FieldInfo fieldInfo )
                return GetSetterLambdaExpression( fieldInfo );

            if( memberInfo is PropertyInfo propertyInfo )
                return GetSetterLambdaExpression( propertyInfo );

            if( memberInfo is MethodInfo methodInfo )
                return GetSetterLambdaExpression( methodInfo );

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
}

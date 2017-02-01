using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public static class FastInvoke
    {
        public static LambdaExpression GetGetterLambdaExpression( this MemberInfo memberInfo )
        {
            if( memberInfo is FieldInfo )
                return GetGetterLambdaExpression( (FieldInfo)memberInfo );

            if( memberInfo is PropertyInfo )
                return GetGetterLambdaExpression( (PropertyInfo)memberInfo );

            throw new ArgumentException( $"Cannot handle {memberInfo}" );
        }

        public static LambdaExpression GetSetterLambdaExpression( this MemberInfo memberInfo )
        {
            if( memberInfo is FieldInfo )
                return GetSetterLambdaExpression( (FieldInfo)memberInfo );

            if( memberInfo is PropertyInfo )
                return GetSetterLambdaExpression( (PropertyInfo)memberInfo );

            throw new ArgumentException( $"Cannot handle {memberInfo}" );
        }

        public static LambdaExpression GetGetterLambdaExpression( this PropertyInfo propertyInfo )
        {
            // (target, value) => target.get_Property()
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

        public static LambdaExpression GetGetterLambdaExpression( this FieldInfo fieldInfo )
        {
            // (target, value) => target.field;

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

        public static Func<T, TReturn> BuildTypedGetter<T, TReturn>( this PropertyInfo propertyInfo )
        {
            return (Func<T, TReturn>)Delegate.CreateDelegate(
                typeof( Func<T, TReturn> ), propertyInfo.GetGetMethod() );
        }

        public static Action<T, TProperty> BuildTypedSetter<T, TProperty>( this PropertyInfo propertyInfo )
        {
            return (Action<T, TProperty>)Delegate.CreateDelegate(
                typeof( Action<T, TProperty> ), propertyInfo.GetSetMethod() );
        }

        public static Action<object, object> BuildUntypedCastSetter( this PropertyInfo propertyInfo )
        {
            // (t, p) => Convert(t).set_Foo( Convert(p) )

            var methodInfo = propertyInfo.GetSetMethod();

            var exTarget = Expression.Parameter( typeof( object ), "t" );
            var exValue = Expression.Parameter( typeof( object ), "p" );

            var convert = Expression.Convert( exTarget, propertyInfo.ReflectedType );
            var assign = Expression.Call( convert, methodInfo,
                Expression.Convert( exValue, propertyInfo.PropertyType ) );

            var exBody = assign;
            var lambda = Expression.Lambda<Action<object, object>>( exBody, exTarget, exValue );

            return lambda.Compile();
        }

        public static Action<T, object> BuildUntypedSetter<T>( this PropertyInfo propertyInfo )
        {
            // (t, p) => if( p != null ) t.set_Foo( Convert(p) )

            var targetType = propertyInfo.ReflectedType;
            var methodInfo = propertyInfo.GetSetMethod();
            var exTarget = Expression.Parameter( targetType, "t" );
            var exValue = Expression.Parameter( typeof( object ), "p" );

            var nullTest = Expression.NotEqual( exValue,
                Expression.Constant( null, typeof( object ) ) );

            var assign = Expression.Call( exTarget, methodInfo,
                Expression.Convert( exValue, propertyInfo.PropertyType ) );

            var exBody = assign;// Expression.Condition( nullTest, assign, Expression.Empty() );
            var lambda = Expression.Lambda<Action<T, object>>( exBody, exTarget, exValue );

            return lambda.Compile();
        }

        public static Action<T, object> BuildUntypedSetter<T, TProperty>( this PropertyInfo propertyInfo )
        {
            if( typeof( TProperty ) != propertyInfo.PropertyType )
            {
                string errorMsg = $"'{nameof( propertyInfo )}' must provide a property matching the type of {nameof( TProperty )}. " +
                   $"Expected type: '{typeof( TProperty ).FullName}', provided type: '{propertyInfo.PropertyType.FullName}'";

                throw new ArgumentException( errorMsg );
            }

            var setter = BuildTypedSetter<T, TProperty>( propertyInfo );
            return new Action<T, object>( ( t, o ) => setter( t, (TProperty)o ) );
        }

        public static Func<object, object> BuildUntypedCastGetter( this PropertyInfo propertyInfo )
        {
            // t => Convert( t.get_Foo() )

            var methodInfo = propertyInfo.GetGetMethod();

            var exTarget = Expression.Parameter( typeof( object ), "t" );
            var convert = Expression.Convert( exTarget, propertyInfo.ReflectedType );
            var exBody = Expression.Call( convert, methodInfo );
            var exBody2 = Expression.Convert( exBody, typeof( object ) );

            var lambda = Expression.Lambda<Func<object, object>>( exBody2, exTarget );
            return lambda.Compile();
        }

        public static Func<T, object> BuildUntypedGetter<T>( this PropertyInfo propertyInfo )
        {
            // t => Convert( t.get_Foo() )

            var targetType = propertyInfo.ReflectedType;
            var methodInfo = propertyInfo.GetGetMethod();
            var returnType = methodInfo.ReturnType;

            var exTarget = Expression.Parameter( targetType, "t" );
            var exBody = Expression.Call( exTarget, methodInfo );
            var exBody2 = Expression.Convert( exBody, typeof( object ) );

            var lambda = Expression.Lambda<Func<T, object>>( exBody2, exTarget );
            return lambda.Compile();
        }

        public static Action<T, VIndex, object> BuildUntypedIndexSetter<T, VIndex>( this PropertyInfo propertyInfo )
        {
            // (t, i, p) => t.set_Foo( Convert(p) )

            var targetType = propertyInfo.PropertyType;
            var indexer = propertyInfo.PropertyType.GetProperty( "Item" );
            if( indexer == null )
                return null;

            var methodInfo = indexer.GetSetMethod();

            var exTarget = Expression.Parameter( typeof( object ), "t" );
            var exIndex = Expression.Parameter( typeof( int ), "i" );
            var exValue = Expression.Parameter( typeof( object ), "p" );

            var exBody = Expression.Call( Expression.Convert( exTarget, targetType ),
                methodInfo, exIndex, Expression.Convert( exValue, indexer.PropertyType ) );

            var lambda = Expression.Lambda<Action<T, VIndex, object>>( exBody, exTarget, exIndex, exValue );
            return lambda.Compile();
        }
    }
}

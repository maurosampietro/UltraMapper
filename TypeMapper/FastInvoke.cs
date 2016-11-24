using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public class FastInvoke
    {
        public static Func<T, TReturn> BuildTypedGetter<T, TReturn>( PropertyInfo propertyInfo )
        {
            return (Func<T, TReturn>)Delegate.CreateDelegate(
                typeof( Func<T, TReturn> ), propertyInfo.GetGetMethod() );
        }

        public static Action<T, TProperty> BuildTypedSetter<T, TProperty>( PropertyInfo propertyInfo )
        {
            return (Action<T, TProperty>)Delegate.CreateDelegate(
                typeof( Action<T, TProperty> ), propertyInfo.GetSetMethod() );
        }

        public static Action<object, object> BuildUntypedCastSetter( PropertyInfo propertyInfo )
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

        public static Action<T, object> BuildUntypedSetter<T>( PropertyInfo propertyInfo )
        {
            // (t, p) => if( p != null ) t.set_Foo( Convert(p) )

            var targetType = propertyInfo.DeclaringType;
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

        public static Action<T, object> BuildUntypedSetter<T, TProperty>( PropertyInfo propertyInfo )
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

        public static Func<object, object> BuildUntypedCastGetter( PropertyInfo propertyInfo )
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

        public static Func<T, object> BuildUntypedGetter<T>( PropertyInfo propertyInfo )
        {
            // t => Convert( t.get_Foo() )

            var targetType = propertyInfo.DeclaringType;
            var methodInfo = propertyInfo.GetGetMethod();
            var returnType = methodInfo.ReturnType;

            var exTarget = Expression.Parameter( targetType, "t" );
            var exBody = Expression.Call( exTarget, methodInfo );
            var exBody2 = Expression.Convert( exBody, typeof( object ) );

            var lambda = Expression.Lambda<Func<T, object>>( exBody2, exTarget );
            return lambda.Compile();
        }

        public static Action<T, VIndex, object> BuildUntypedIndexSetter<T, VIndex>( PropertyInfo propertyInfo )
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

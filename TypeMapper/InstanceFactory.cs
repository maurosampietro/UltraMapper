using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TypeMapper
{
    public class InstanceFactory
    {
        public static T CreateObject<T>( params object[] constructorValues )
        {
            return (T)CreateObject( typeof( T ), constructorValues );
        }

        public static object CreateObject( Type intanceType, params object[] constructorValues )
        {
            //parameterless
            if( constructorValues == null || constructorValues.Length == 0 )
            {
                var instanceCreator = ConstructorFactory.CreateConstructor( intanceType );
                return instanceCreator();
            }
            else // with parameters
            {
                var paramTypes = constructorValues.Select( value => value.GetType() ).ToArray();
                var instanceCreator = ConstructorFactory.CreateConstructor( intanceType, paramTypes );

                return instanceCreator( constructorValues );
            }
        }
    }

    public class ConstructorFactory
    {
        public static Func<object> CreateConstructor( Type type )
        {
            var instanceCreatorExp = Expression.Lambda<Func<object>>(
                Expression.Convert( Expression.New( type ), typeof( object ) ) );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, object> CreateConstructor<TArg1>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ) };
            var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, object>>(
                Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, object> CreateConstructor<TArg1, TArg2>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ) };
            var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, object>>(
                Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TArg3, object> CreateConstructor<TArg1, TArg2, TArg3>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ) };
            var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(
               Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, object> CreateConstructor<TArg1, TArg2, TArg3, TArg4>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4)};

            var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, object>>(
               Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, object> CreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4), typeof(TArg5) };

            var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>>(
               Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<object[], object> CreateConstructor( Type type, params Type[] ctorArgTypes )
        {
            if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                ctorArgTypes = Type.EmptyTypes;

            var constructorInfo = type.GetConstructor( ctorArgTypes );

            var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
            var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<object[], object>>(
               Expression.Convert( Expression.New( constructorInfo, constructorArgs ), typeof( object ) ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TInstance> CreateConstructor<TInstance>()
        {
            var instanceType = typeof( TInstance );

            var instanceCreatorExp = Expression.Lambda<Func<TInstance>>(
                    Expression.New( typeof( TInstance ) ) );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TInstance> CreateConstructor<TArg1, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ) };
            var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TInstance>>(
                Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TInstance> CreateConstructor<TArg1, TArg2, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ) };
            var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TInstance>>(
                Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TArg3, TInstance> CreateConstructor<TArg1, TArg2, TArg3, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ) };
            var instanceType = typeof( TInstance );

            var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TInstance>>(
                Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TInstance> CreateConstructor<TArg1, TArg2, TArg3, TArg4, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4)};

            var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TInstance>>(
                Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance> CreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4), typeof(TArg5) };

            var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

            var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>>(
                Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }

        public static Func<object[], TInstance> CreateConstructor<TInstance>( params Type[] ctorArgTypes )
        {
            if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                ctorArgTypes = Type.EmptyTypes;

            var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

            var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
            var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

            var instanceCreatorExp = Expression.Lambda<Func<object[], TInstance>>(
               Expression.New( constructorInfo, constructorArgs ), lambdaArgs );

            return instanceCreatorExp.Compile();
        }
    }
}

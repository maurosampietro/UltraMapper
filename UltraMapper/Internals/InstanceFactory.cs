using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper
{
    public class InstanceFactory
    {
        #region Instance type passed as generic param
        public static TReturn CreateObject<TArg1, TReturn>( TArg1 arg1 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TReturn>();
            return instanceCreator( arg1 );
        }

        public static TReturn CreateObject<TArg1, TArg2, TReturn>( TArg1 arg1, TArg2 arg2 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TReturn>();
            return instanceCreator( arg1, arg2 );
        }

        public static TReturn CreateObject<TArg1, TArg2, TArg3, TReturn>( TArg1 arg1, TArg2 arg2, TArg3 arg3 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TArg3, TReturn>();
            return instanceCreator( arg1, arg2, arg3 );
        }

        public static TReturn CreateObject<TArg1, TArg2, TArg3, TArg4, TReturn>( TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TArg3, TArg4, TReturn>();
            return instanceCreator( arg1, arg2, arg3, arg4 );
        }

        public static TReturn CreateObject<TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>( TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TReturn>();
            return instanceCreator( arg1, arg2, arg3, arg4, arg5 );
        }

        public static TReturn CreateObject<TReturn>( params object[] constructorValues )
        {
            return (TReturn)CreateObject( typeof( TReturn ), constructorValues );
        }
        #endregion

        #region Instance type passed as type param

        public static object CreateObject( Type instanceType )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor( instanceType );
            return instanceCreator();
        }

        public static object CreateObject<TArg1>( Type instanceType, TArg1 arg1 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1>( instanceType );
            return instanceCreator( arg1 );
        }

        public static object CreateObject<TArg1, TArg2>( Type instanceType, TArg1 arg1, TArg2 arg2 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2>( instanceType );
            return instanceCreator( arg1, arg2 );
        }

        public static object CreateObject<TArg1, TArg2, TArg3>( Type instanceType, TArg1 arg1, TArg2 arg2, TArg3 arg3 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TArg3>( instanceType );
            return instanceCreator( arg1, arg2, arg3 );
        }

        public static object CreateObject<TArg1, TArg2, TArg3, TArg4>( Type instanceType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TArg3, TArg4>( instanceType );
            return instanceCreator( arg1, arg2, arg3, arg4 );
        }

        public static object CreateObject<TArg1, TArg2, TArg3, TArg4, TArg5>( Type instanceType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5 )
        {
            var instanceCreator = ConstructorFactory.CreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5>( instanceType );
            return instanceCreator( arg1, arg2, arg3, arg4, arg5 );
        }

        public static object CreateObject( Type intanceType, params object[] constructorValues )
        {
            //parameterless
            if( constructorValues == null || constructorValues.Length == 0 )
            {
                var instanceCreator = ConstructorFactory.CreateConstructor( intanceType );
                return instanceCreator();
            }

            //else //with parameters
            {
                var paramTypes = constructorValues.Select( value => value.GetType() ).ToArray();
                var instanceCreator = ConstructorFactory.CreateConstructor( intanceType, paramTypes );

                return instanceCreator( constructorValues );
            }
        }
        #endregion
    }

    internal partial class ConstructorFactory
    {
        private static readonly Dictionary<int, Delegate> _cacheWeakTyped = new();
        private static readonly Dictionary<int, Delegate> _cacheStrongTyped = new();

        private static int GetArrayHashCode<T>( IEnumerable<T> array )
        {
            /* 
             * The hashcode function must take into account that
             * a ctor overload taking as input the same number of params of the same type
             * but in different order must produce different hashcodes. 
             *
             *  eg: should produce different hashcodes for this scenario:
             *  class 
             *  {
             *      ctor(int,string);
             *      ctor(string,int);           
             *  }
            */

            return array.Select( item => item.GetHashCode() )
                .Aggregate( ( agg, current ) => agg * 11 ^ current.GetHashCode() );
        }

        #region Instance type passed as type param
        public static Func<object> CreateConstructor( Type instanceType )
        {
            var typesInvolved = new[] { instanceType, typeof( object ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                var instanceCreatorExp = Expression.Lambda<Func<object>>(
                    Expression.Convert( Expression.New( instanceType ), typeof( object ) ) );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, object> CreateConstructor<TArg1>( Type instanceType )
        {
            var typesInvolved = new[] { instanceType, typeof( TArg1 ), typeof( object ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ) };
                var ctorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, object>>(
                Expression.Convert( Expression.New( ctorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, object> CreateConstructor<TArg1, TArg2>( Type instanceType )
        {
            var typesInvolved = new[] { instanceType, typeof( TArg1 ), typeof( TArg2 ), typeof( object ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ) };
                var ctorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, object>>(
                    Expression.Convert( Expression.New( ctorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TArg3, object> CreateConstructor<TArg1, TArg2, TArg3>( Type instanceType )
        {
            var typesInvolved = new[] { instanceType, typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ), typeof( object ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TArg3, object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ) };
                var ctorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(
                Expression.Convert( Expression.New( ctorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, object> CreateConstructor<TArg1, TArg2, TArg3, TArg4>( Type instanceType )
        {
            var typesInvolved = new[] { instanceType, typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ), typeof( TArg4 ), typeof( object ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TArg3, TArg4, object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                    typeof( TArg3 ), typeof( TArg4 ) };

                var ctorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, object>>(
                Expression.Convert( Expression.New( ctorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, object> CreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5>( Type instanceType )
        {
            var typesInvolved = new[] { instanceType, typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ), typeof( TArg4 ), typeof( TArg5 ), typeof( object ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                    typeof( TArg3 ), typeof(TArg4), typeof(TArg5) };

                var ctorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>>(
                Expression.Convert( Expression.New( ctorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<object[], object> CreateConstructor( Type instanceType, params Type[] ctorArgTypes )
        {
            var typesInvolved = new[] { instanceType }.Concat( ctorArgTypes );
            var key = GetArrayHashCode( typesInvolved );

            return (Func<object[], object>)_cacheWeakTyped.GetOrAdd( key, () =>
            {
                if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                    ctorArgTypes = Type.EmptyTypes;

                var ctorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
                var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                    Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<object[], object>>(
                   Expression.Convert( Expression.New( ctorInfo, constructorArgs ), typeof( object ) ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }
        #endregion

        #region Instance type passed as generic param
        public static Func<TInstance> CreateConstructor<TInstance>()
        {
            var typesInvolved = new[] { typeof( TInstance ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                var instanceCreatorExp = Expression.Lambda<Func<TInstance>>(
                    Expression.New( typeof( TInstance ) ) );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TInstance> CreateConstructor<TArg1, TInstance>()
        {
            var typesInvolved = new[] { typeof( TInstance ), typeof( TArg1 ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ) };
                var ctorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TInstance>>(
                    Expression.New( ctorInfo, lambdaArgs ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TInstance> CreateConstructor<TArg1, TArg2, TInstance>()
        {
            var typesInvolved = new[] { typeof( TInstance ), typeof( TArg1 ), typeof( TArg2 ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ) };
                var ctorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TInstance>>(
                    Expression.New( ctorInfo, lambdaArgs ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TArg3, TInstance> CreateConstructor<TArg1, TArg2, TArg3, TInstance>()
        {
            var typesInvolved = new[] { typeof( TInstance ), typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TArg3, TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ) };
                var ctorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TInstance>>(
                    Expression.New( ctorInfo, lambdaArgs ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TInstance> CreateConstructor<TArg1, TArg2, TArg3, TArg4, TInstance>()
        {
            var typesInvolved = new[] { typeof( TInstance ), typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ), typeof( TArg4 ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TArg3, TArg4, TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                    typeof( TArg3 ), typeof(TArg4)};

                var ctorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TInstance>>(
                    Expression.New( ctorInfo, lambdaArgs ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance> CreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>()
        {
            var typesInvolved = new[] { typeof( TInstance ), typeof( TArg1 ), typeof( TArg2 ), typeof( TArg3 ), typeof( TArg4 ), typeof( TArg5 ) };
            var key = GetArrayHashCode( typesInvolved );

            return (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                    typeof( TArg3 ), typeof(TArg4), typeof(TArg5) };

                var ctorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>>(
                    Expression.New( ctorInfo, lambdaArgs ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }

        public static Func<object[], TInstance> CreateConstructor<TInstance>( params Type[] ctorArgTypes )
        {
            var typesInvolved = new[] { typeof( TInstance ) }.Concat( ctorArgTypes );
            var key = GetArrayHashCode( typesInvolved );

            return (Func<object[], TInstance>)_cacheStrongTyped.GetOrAdd( key, () =>
            {
                if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                    ctorArgTypes = Type.EmptyTypes;

                var ctorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
                var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                    Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<object[], TInstance>>(
                   Expression.New( ctorInfo, constructorArgs ), lambdaArgs );

                return instanceCreatorExp.Compile();
            } );
        }
        #endregion
    }
}

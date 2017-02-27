using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TypeMapper
{
    //STUB
    public class InstanceFactory
    {
        public static T CreateObject<T>( params object[] constructorValues )
        {
            return (T)CreateObject( typeof( T ), constructorValues );
        }

        public static object CreateObject( Type type, params object[] constructorValues )
        {
            //parameterless
            if( constructorValues == null || constructorValues.Length == 0 )
            {
                var instanceCreator = ConstructorFactory.GetOrCreateConstructor( type );
                return instanceCreator();
            }
            else // with parameters
            {
                //var paramTypes = constructorValues.Select( v => v.GetType() );
                var instanceCreator = ConstructorFactory.GetOrCreateConstructor( type );
                return instanceCreator();
            }
        }
    }

    public class ConstructorFactory
    {
        private struct ConstructorArgs
        {
            public readonly Type[] ArgTypes;
            private int _hashCode;

            public ConstructorArgs( params Type[] argTypes )
            {
                this.ArgTypes = argTypes;

                if( this.ArgTypes == null ) _hashCode = 0;
                else
                {
                    _hashCode = this.ArgTypes.Aggregate( 31, ( xorAcc, argType )
                        => xorAcc ^= argType.GetHashCode() );
                }
            }

            public override bool Equals( object obj )
            {
                if( obj == null ) return false;
                var instance = (ConstructorArgs)obj;

                if( Object.ReferenceEquals( this.ArgTypes, instance.ArgTypes ) )
                    return true;

                if( this.ArgTypes.Length != instance.ArgTypes.Length )
                    return false;

                for( int i = 0; i < this.ArgTypes.Length; i++ )
                {
                    if( this.ArgTypes[ i ] != instance.ArgTypes[ i ] )
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public static bool operator ==( ConstructorArgs objA, ConstructorArgs objB )
            {
                return objA.Equals( objB );
            }
            public static bool operator !=( ConstructorArgs objA, ConstructorArgs objB )
            {
                return !objA.Equals( objB );
            }
        }

        private struct CacheKey
        {
            public readonly Type InstanceType;
            public readonly ConstructorArgs ContructorArgs;

            private int _hashCode;

            public CacheKey( Type instanceType, params Type[] constructorArgTypes )
            {
                this.InstanceType = instanceType;
                this.ContructorArgs = new ConstructorArgs( constructorArgTypes );

                _hashCode = this.ContructorArgs.GetHashCode() ^
                    this.InstanceType.GetHashCode();
            }

            public override bool Equals( object obj )
            {
                if( obj == null ) return false;
                var instance = (CacheKey)obj;

                return this.InstanceType == instance.InstanceType &&
                    this.ContructorArgs == instance.ContructorArgs;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        /// <summary>
        /// Caches delegate with typed return type
        /// </summary>
        private static Dictionary<CacheKey, Delegate> _cacheTyped
             = new Dictionary<CacheKey, Delegate>();

        /// <summary>
        /// Caches delegate with untyped (object) return type 
        /// </summary>
        private static Dictionary<CacheKey, Delegate> _cacheUntyped
            = new Dictionary<CacheKey, Delegate>();

        public static Func<object> GetOrCreateConstructor( Type type )
        {
            var cacheKey = new CacheKey( type, null );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                //2. generate one otherwise
                var instanceCreatorExp = Expression.Lambda<Func<object>>(
                    Expression.Convert( Expression.New( type ), typeof( object ) ) );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheUntyped.Add( cacheKey, instanceCreator );
            }

            return (Func<object>)instanceCreator;
        }

        public static Func<TArg1, object> GetOrCreateConstructor<TArg1>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ) };

            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, object>>(
                    Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheUntyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, object>)instanceCreator;
        }

        public static Func<TArg1, TArg2, object> GetOrCreateConstructor<TArg1, TArg2>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ) };

            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, object>>(
                    Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheUntyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, object>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TArg3, object> GetOrCreateConstructor<TArg1, TArg2, TArg3>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 )};
            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, object>>(
                   Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );


                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheUntyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TArg3, object>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, object> GetOrCreateConstructor<TArg1, TArg2, TArg3, TArg4>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4)};

            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, object>>(
                   Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheUntyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TArg3, TArg4, object>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, object> GetOrCreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5>( Type instanceType )
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4), typeof(TArg5) };

            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>>(
                   Expression.Convert( Expression.New( constructorInfo, lambdaArgs ), typeof( object ) ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheUntyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TArg3, TArg4, TArg5, object>)instanceCreator;
        }

        public static Func<object[], object> GetOrCreateConstructor( Type type, params Type[] ctorArgTypes )
        {
            if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                ctorArgTypes = Type.EmptyTypes;

            var cacheKey = new CacheKey( type, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            //if( !_cacheUntyped.TryGetValue( cacheKey, out instanceCreator ) )
            //{
                //2. generate one otherwise
                var constructorInfo = type.GetConstructor( ctorArgTypes );
            
                var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
                var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                    Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<object[], object>>(
                   Expression.Convert( Expression.New( constructorInfo, constructorArgs ), typeof( object ) ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
            //    _cacheUntyped.Add( cacheKey, instanceCreator );
            //}

            return (Func<object[], object>)instanceCreator;
        }

        public static Func<TInstance> GetOrCreateConstructor<TInstance>()
        {
            var instanceType = typeof( TInstance );
            var cacheKey = new CacheKey( instanceType, null );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                instanceCreator = Expression.Lambda<Func<TInstance>>(
                    Expression.New( typeof( TInstance ) ) ).Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TInstance>)instanceCreator;
        }

        public static Func<TArg1, TInstance> GetOrCreateConstructor<TArg1, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ) };

            var instanceType = typeof( TInstance );
            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TInstance>>(
                    Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TInstance>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TInstance> GetOrCreateConstructor<TArg1, TArg2, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ) };

            var instanceType = typeof( TInstance );
            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TInstance>>(
                    Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TInstance>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TArg3, TInstance> GetOrCreateConstructor<TArg1, TArg2, TArg3, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 )};
            var instanceType = typeof( TInstance );
            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TInstance>>(
                    Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TArg3, TInstance>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TInstance> GetOrCreateConstructor<TArg1, TArg2, TArg3, TArg4, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4)};

            var instanceType = typeof( TInstance );
            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = typeof( TInstance ).GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TInstance>>(
                    Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TArg3, TArg4, TInstance>)instanceCreator;
        }

        public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance> GetOrCreateConstructor<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>()
        {
            var ctorArgTypes = new[] { typeof( TArg1 ), typeof( TArg2 ),
                typeof( TArg3 ), typeof(TArg4), typeof(TArg5) };

            var instanceType = typeof( TInstance );
            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = ctorArgTypes.Select( ( type, index ) =>
                    Expression.Parameter( type, $"p{index}" ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>>(
                    Expression.New( constructorInfo, lambdaArgs ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<TArg1, TArg2, TArg3, TArg4, TArg5, TInstance>)instanceCreator;
        }

        public static Func<object[], TInstance> GetOrCreateConstructor<TInstance>( params Type[] ctorArgTypes )
        {
            var instanceType = typeof( TInstance );

            if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                ctorArgTypes = Type.EmptyTypes;

            var cacheKey = new CacheKey( instanceType, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Delegate instanceCreator;
            if( !_cacheTyped.TryGetValue( cacheKey, out instanceCreator ) )
            {
                //2. generate one otherwise
                var constructorInfo = instanceType.GetConstructor( ctorArgTypes );

                var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
                var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                    Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<object[], TInstance>>(
                   Expression.New( constructorInfo, constructorArgs ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheTyped.Add( cacheKey, instanceCreator );
            }

            return (Func<object[], TInstance>)instanceCreator;
        }
    }
}

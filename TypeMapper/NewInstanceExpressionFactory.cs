using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public class InstanceFactory
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
        /// Constructor with parameters cache
        /// </summary>
        private static Dictionary<CacheKey, Func<object[], object>> _cache
             = new Dictionary<CacheKey, Func<object[], object>>();

        /// <summary>
        /// Parameterless constructors cache
        /// </summary>
        private static Dictionary<CacheKey, Func<object>> _cacheNoParams
             = new Dictionary<CacheKey, Func<object>>();


        public static object CreateObject( Type type, params object[] constructorValues )
        {
            return GetOrCreateConstructor( type, constructorValues )( constructorValues );           
        }

        #region Parameterless Constructor
        public static Func<object> GetOrCreateConstructor( Type type )
        {
            var cacheKey = new CacheKey( type, null );

            //1. look in the cache if a suitable constructor has already been generated
            Func<object> instanceCreator;
            if( !_cacheNoParams.TryGetValue( cacheKey, out instanceCreator ) )
            {
                //2. generate one otherwise
                var constructorInfo = type.GetConstructor( Type.EmptyTypes );

                var instanceCreatorExp = Expression.Lambda<Func<object>>(
                    Expression.Convert( Expression.New( constructorInfo ), typeof( object ) ) );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cacheNoParams.Add( cacheKey, instanceCreator );
            }

            return instanceCreator;
        }
        #endregion

        #region Construtor with parameters

        private static Func<object[], object> GetOrCreateConstructor( Type type, params object[] values )
        {
            var ctorArgTypes = type.GetGenericArguments();
            return GetOrCreateConstructor( type, ctorArgTypes );
        }

        private static Func<object[], object> GetOrCreateConstructor( Type type, params Type[] ctorArgTypes )
        {
            if( ctorArgTypes == null || ctorArgTypes.Length == 0 )
                ctorArgTypes = Type.EmptyTypes;

            var cacheKey = new CacheKey( type, ctorArgTypes );

            //1. look in the cache if a suitable constructor has already been generated
            Func<object[], object> instanceCreator;
            if( !_cache.TryGetValue( cacheKey, out instanceCreator ) )
            {
                //2. generate one otherwise
                var constructorInfo = type.GetConstructor( ctorArgTypes );

                var lambdaArgs = Expression.Parameter( typeof( object[] ), "args" );
                var constructorArgs = ctorArgTypes.Select( ( t, i ) => Expression.Convert(
                    Expression.ArrayIndex( lambdaArgs, Expression.Constant( i ) ), t ) ).ToArray();

                var instanceCreatorExp = Expression.Lambda<Func<object[], object>>(
                   Expression.Convert( Expression.New( constructorInfo, constructorArgs ), typeof( object ) ), lambdaArgs );

                instanceCreator = instanceCreatorExp.Compile();

                //3. cache it
                _cache.Add( cacheKey, instanceCreator );
            }

            return instanceCreator;
        } 
        #endregion
    }
}

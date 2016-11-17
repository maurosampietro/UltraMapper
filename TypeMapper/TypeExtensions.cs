using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Check if a type is a base type, optionally unwrapping 
        /// and checking the underlying type of a Nullable (which is not a base type).
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <param name="unwrapNullableTypes">If true, checks the underlying type of a Nullable.</param>
        /// <returns>True if the type is a base type, false otherwise.</returns>
        public static bool IsBuiltInType( this Type type, bool unwrapNullableTypes )
        {
            var checkType = type;
            if( unwrapNullableTypes )
                checkType = type.GetUnderlyingTypeIfNullable();

            return checkType == typeof( bool ) ||
                checkType == typeof( byte ) ||
                checkType == typeof( sbyte ) ||
                checkType == typeof( char ) ||
                checkType == typeof( decimal ) ||
                checkType == typeof( double ) ||
                checkType == typeof( float ) ||
                checkType == typeof( int ) ||
                checkType == typeof( uint ) ||
                checkType == typeof( long ) ||
                checkType == typeof( ulong ) ||
                checkType == typeof( object ) ||
                checkType == typeof( short ) ||
                checkType == typeof( ushort ) ||
                checkType == typeof( string );
        }

        /// <summary>
        /// Checks if the type is nullable and return the underlying type if it is.
        /// If the type is not a nullable the type itself is returned.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <returns>The underlying type if <paramref name="type"/> is Nullable; <paramref name="type"/> itself otherwise.</returns>
        public static Type GetUnderlyingTypeIfNullable( this Type type )
        {
            var nullableType = Nullable.GetUnderlyingType( type );
            return nullableType == null ? type : nullableType;
        }

        public static bool IsEnumerable( this Type type )
        {
            return type.GetInterfaces().Any( t => t == typeof( IEnumerable ) );
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace UltraMapper.Internals
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Check if a type is a base type, optionally unwrapping 
        /// and checking the underlying type of a Nullable (which is not a base type).
        /// Object is not considered a base type.
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
                //checkType == typeof( object ) //
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

        public static bool IsNullable( this Type type )
        {
            return Nullable.GetUnderlyingType( type ) != null;
        }

        public static bool IsEnumerable( this Type type )
        {
            return type.GetInterfaces().Any( t => t == typeof( IEnumerable ) );
        }

        public static object GetDefaultValueViaActivator( this Type type )
        {
            if( type.IsValueType && Nullable.GetUnderlyingType( type ) == null )
                return Activator.CreateInstance( type );

            return null;
        }

        public static Type GetCollectionGenericType( this Type type )
        {
            foreach( Type interfaceType in type.GetInterfaces() )
            {
                if( interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
                {
                    return interfaceType.GetGenericArguments()[ 0 ];
                }
            }

            return null;
        }

        public static bool ImplementsInterface( this Type sourceType, Type @interface )
        {
            if( !@interface.IsInterface )
                throw new ArgumentException( $"{nameof( @interface )} parameter must be an interface type" );

            if( @interface.IsGenericTypeDefinition )
            {
                return sourceType.GetInterfaces().Any( type => type.IsGenericType &&
                    type.GetGenericTypeDefinition() == @interface );
            }

            return sourceType.GetInterfaces().Any( type => type == @interface );
        }

        public static bool IsCollectionOfType( this Type sourceType, Type testAgainstType )
        {
            return sourceType.IsGenericType && (sourceType.GetGenericTypeDefinition() == testAgainstType);
        }

        #region IsImplicitlyConvertibleTo

        private class TypePair
        {
            public readonly Type SourceType;
            public readonly Type DestinationType;
            private readonly int _hashcode;

            public TypePair( Type sourceType, Type destinatinationType )
            {
                this.SourceType = sourceType;
                this.DestinationType = destinatinationType;

                _hashcode = unchecked(SourceType.GetHashCode() * 31)
                    ^ DestinationType.GetHashCode();
            }

            public override bool Equals( object obj )
            {
                var typePair = obj as TypePair;
                if( typePair == null ) return false;

                return this.SourceType.Equals( typePair.SourceType ) &&
                    this.DestinationType.Equals( typePair.DestinationType );
            }

            public override int GetHashCode()
            {
                return _hashcode;
            }
        }

        /// <summary>
        /// A cache that maps a type to the types to which you can implicitly convert.
        /// </summary>
        private static Dictionary<TypePair, bool> _implicitConversionTable = new Dictionary<TypePair, bool>();

        /// <summary>
        /// A cache that maps a numeric built-in type to the numeric built-in types to which you can implicitly convert.
        /// <a href="https://msdn.microsoft.com/en-us/library/y5b434w4.aspx">Implicit numeric conversion table</a>
        /// </summary>
        private static readonly ReadOnlyDictionary<Type, HashSet<Type>> _implicitNumericConversionTable = new ReadOnlyDictionary<Type, HashSet<Type>>( new Dictionary<Type, HashSet<Type>>()
        {
             {typeof( sbyte ), new HashSet<Type>(){ typeof( short ), typeof( int ), typeof( long ), typeof( float ), typeof( double ), typeof( decimal ) }},
             {typeof( byte ), new HashSet<Type>() { typeof( short ), typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) }},
             {typeof( short), new HashSet<Type>(){ typeof( int ), typeof( long ), typeof( float ), typeof( double ), typeof( decimal )}},
             {typeof( ushort ), new HashSet<Type>(){ typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) }},
             {typeof( int ), new HashSet<Type>(){ typeof( long ), typeof( float ), typeof( double ), typeof( decimal ) }},
             {typeof( uint ), new HashSet<Type>(){ typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) }},
             {typeof( long ), new HashSet<Type>(){ typeof( float ), typeof( double ), typeof( decimal ) }},
             {typeof( char ), new HashSet<Type>(){ typeof( ushort ), typeof( int ), typeof( uint ), typeof( long ), typeof( ulong ), typeof( float ), typeof( double ), typeof( decimal ) } },
             {typeof( float ), new HashSet<Type>(){ typeof( double ) }},
             {typeof( ulong ), new HashSet<Type>(){ typeof( float ), typeof( double ), typeof( decimal ) }},
        } );

        /// <summary>
        /// Check if a implicit numeric conversion (no information loss) is available as documented 
        /// <a href="https://msdn.microsoft.com/en-us/library/y5b434w4.aspx">here</a>
        /// </summary>
        /// <param name="sourceType">the type being tested.</param>
        /// <param name="targetType">the target type to test against.</param>
        /// <returns>True if a implicit conversion is available, false otherwise</returns>
        public static bool IsImplicitlyConvertibleTo( this Type sourceType, Type targetType )
        {
            //check implicit conversion between built-in types
            HashSet<Type> implicitConversions;
            if( _implicitNumericConversionTable.TryGetValue( sourceType, out implicitConversions ) )
                return implicitConversions.Contains( targetType );

            //check implicit conversion from any type to any type
            bool conversionExists = false;
            var typePairKey = new TypePair( sourceType, targetType );

            if( !_implicitConversionTable.TryGetValue( typePairKey, out conversionExists ) )
            {
                conversionExists = sourceType.GetMethods( BindingFlags.Public | BindingFlags.Static )
                      .Where( methodInfo => methodInfo.Name == "op_Implicit" && methodInfo.ReturnType == targetType )
                      .Any( methodInfo =>
                      {
                          ParameterInfo paramInfo = methodInfo.GetParameters().FirstOrDefault();
                          return paramInfo?.ParameterType == sourceType;
                      } );

                //cache the result
                _implicitConversionTable.Add( typePairKey, conversionExists );
            }

            return conversionExists;
        }

        #endregion

        #region IsExplicitlyConvertibleTo
        /// <summary>
        /// A cache that maps a type to the types to which you can explicitly convert.
        /// </summary>
        private static Dictionary<TypePair, bool> _explicitConversionTable = new Dictionary<TypePair, bool>();

        /// <summary>
        /// A cache that maps a numeric built-in type to the numeric built-in types to which you can explicitly convert.
        /// <a href="https://msdn.microsoft.com/en-us/library/yht2cx7b.aspx">Explicit numeric conversion table</a>
        /// </summary>
        private static readonly ReadOnlyDictionary<Type, HashSet<Type>> _explicitNumericConversionTable = new ReadOnlyDictionary<Type, HashSet<Type>>( new Dictionary<Type, HashSet<Type>>()
        {
            {typeof( sbyte ), new HashSet<Type>(){ typeof( byte ), typeof( ushort), typeof( uint), typeof( ulong), typeof( char ) } },
            {typeof( byte ), new HashSet<Type>() {typeof( sbyte), typeof( char)} },
            {typeof( short), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( ushort), typeof( uint), typeof( ulong), typeof( char) } },
            {typeof( ushort ), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( short), typeof( char ) } },
            {typeof( int ), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( short), typeof( ushort), typeof( uint), typeof( ulong), typeof( char ) } },
            {typeof( uint ), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( short), typeof( ushort), typeof( int), typeof( char) } },
            {typeof( long ), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( short), typeof( ushort), typeof( int), typeof( uint), typeof( ulong), typeof( char ) } },
            {typeof( ulong ), new HashSet<Type>(){typeof( sbyte), typeof( byte), typeof( short), typeof( ushort), typeof( int), typeof( uint), typeof( long), typeof( char ) } },
            {typeof( char ), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( short )} },
            {typeof( float ), new HashSet<Type>(){ typeof( sbyte), typeof( byte), typeof( short), typeof( ushort), typeof( int), typeof( uint), typeof( long), typeof( ulong), typeof( char), typeof( decimal ) } },
            {typeof( double ), new HashSet<Type>(){typeof( sbyte), typeof( byte), typeof( short), typeof( ushort), typeof( int), typeof( uint), typeof( long), typeof( ulong), typeof( char), typeof( float), typeof( decimal ) } },
            {typeof( decimal ), new HashSet<Type>(){ typeof( sbyte ), typeof( byte), typeof( short), typeof( ushort), typeof( int), typeof( uint), typeof( long), typeof( ulong), typeof( char), typeof( float), typeof( double) }},
        } );

        public static bool IsExplicitlyConvertibleTo( this Type sourceType, Type targetType )
        {
            //check explicit conversion between built-in types
            HashSet<Type> explicitConversions;
            if( _explicitNumericConversionTable.TryGetValue( sourceType, out explicitConversions ) )
                return explicitConversions.Contains( targetType );

            //check explicit conversion from any type to any type
            bool conversionExists = false;
            var typePairKey = new TypePair( sourceType, targetType );

            if( !_explicitConversionTable.TryGetValue( typePairKey, out conversionExists ) )
            {
                conversionExists = sourceType.GetMethods( BindingFlags.Public | BindingFlags.Static )
                      .Where( methodInfo => methodInfo.Name == "op_Explicit" && methodInfo.ReturnType == targetType )
                      .Any( methodInfo =>
                      {
                          ParameterInfo paramInfo = methodInfo.GetParameters().FirstOrDefault();
                          return paramInfo?.ParameterType == sourceType;
                      } );

                //cache the result
                _explicitConversionTable.Add( typePairKey, conversionExists );
            }

            return conversionExists;
        }

        #endregion

        /// <summary>
        /// If a type is generic, gets a prettified name.
        /// If a type is not generic returns its name (type.Name)
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Prettified name of the type</returns>
        public static string GetPrettifiedName( this Type type )
        {
            if( type.IsGenericType )
            {
                var mainType = type.Name.Substring( 0, type.Name.IndexOf( '`' ) );
                var genericArgs = type.GenericTypeArguments.Select( GetPrettifiedName );

                return $"{mainType}<{String.Join( ", ", genericArgs )}>";
            }

            return type.Name;
        }

        public static Type GetType( TypeCode code )
        {
            return Type.GetType( "System." + Enum.GetName( typeof( TypeCode ), code ) );
        }
    }
}

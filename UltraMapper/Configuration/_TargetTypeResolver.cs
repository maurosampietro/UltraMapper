using System;
using System.Collections.Generic;
using UltraMapper.Internals;

namespace UltraMapper
{
    ///// <summary>
    ///// Resolves what type to use as a target type.
    ///// The type will eventually be instantiated.
    ///// It takes into account configuration, mapping configuration, runtime type resolving.
    ///// </summary>
    //public class TargetTypeResolver : ITargetTypeResolver
    //{
    //    public Type GetTargetTypeToInstantiate( Mapping mapping )
    //    {
    //        if( mapping is IMemberMappingOptions mapOptions )
    //        {
    //            var returnType = mapOptions.CustomTargetConstructor.Compile().GetType().GetMethod( "Invoke" ).ReturnType;
    //            if( mapOptions.CustomTargetConstructor.Compile().GetType().GetMethod("Invoke").ReturnType != null )
    //                return returnType;
    //        }
    //    }
    //}

    //public interface ITargetTypeResolver
    //{
    //}

    /// <summary>
    /// When mapping providing both source an target instance, ultramapper gets the 
    /// source and target runtime types and use those for the mapping process.
    /// 
    /// When mapping without providing a target instance, but only a target type,
    /// the target type must be instantiable. 
    /// It must have a ctor or a way to construct the type must be provided.
    /// 
    /// When a non-concrete type (abstract or interface type) is used as target type,
    /// or when in case of a concrete type a way to construct the type is not available,
    /// an exception is thrown.
    /// 
    /// Here we can define a 'map' of concrete types to use when a certain type is encountered. 
    /// </summary>
    public class TargetTypeConfiguration
    {
        private readonly Dictionary<Type, Type> _targetTypes = new Dictionary<Type, Type>();

        public void Add( Type mappingTargetType, Type typeToInstantiate, Func<object> instanceProvider )
        {
            CheckThrow( mappingTargetType, typeToInstantiate );
            _targetTypes.Add( mappingTargetType, typeToInstantiate );
        }

        private static void CheckThrow( Type mappingTargetType, Type typeToInstantiate )
        {
            if( typeToInstantiate.IsAbstract || typeToInstantiate.IsInterface )
                throw new ArgumentException( $"{nameof( typeToInstantiate )} must be a concrete (non-abstract, non-interface) type" );

            //if( typeToInstantiate.GetConstructor(Type.EmptyTypes)== null && instanceProvider == null )
            //    throw new ArgumentException( $"{nameof( typeToInstantiate )} must have a default ctor or you must provide a way to ctor the type using the {nameof(instanceProvider)} param" );

            if( !mappingTargetType.IsAssignableFrom( typeToInstantiate ) )
                throw new ArgumentException( $"{nameof( typeToInstantiate )} must be assignable to {nameof( mappingTargetType )}" );
        }

        public void Remove( Type mappingTargetType )
        {
            _targetTypes.Remove( mappingTargetType );
        }

        public Type this[ Type target ]
        {
            get => _targetTypes[ target ];
            set
            {
                CheckThrow( target, value );
                _targetTypes[ target ] = value;
            }
        }
    }
}

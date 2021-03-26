using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class SourceMemberProvider : IMemberProvider
    {
        public bool IgnoreFields { get; set; } = true;
        public bool IgnoreProperties { get; set; } = false;
        public bool IgnoreMethods { get; set; } = false;
        public bool IgnoreNonPublicMembers { get; set; } = true;

        /// <summary>
        /// Sets BindingFlags.FlattenHierarchy
        /// </summary>
        public bool FlattenHierarchy { get; set; } = true;

        /// <summary>
        /// Sets BindingFlags.DeclaredOnly
        /// </summary>
        public bool DeclaredOnly { get; set; } = false;

        /// <summary>
        /// Consider only methods that are parameterless and return void
        /// </summary>
        public bool AllowGetterMethodsOnly { get; set; } = true;

        public IEnumerable<MemberInfo> GetMembers( Type type )
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if( this.FlattenHierarchy ) bindingFlags |= BindingFlags.FlattenHierarchy;
            if( this.DeclaredOnly ) bindingFlags |= BindingFlags.DeclaredOnly;

            if( !this.IgnoreNonPublicMembers ) bindingFlags |= BindingFlags.NonPublic;

            if( !this.IgnoreFields )
            {
                //- In case of an interface type we do nothing special since interfaces do not support fields
                //- Notice that we don't check for readonly fields: we only need to read from the source!

                var sourceFields = type.GetFields( bindingFlags )
                    .Select( field => field );

                foreach( var field in sourceFields ) yield return field;
            }

            if( !this.IgnoreProperties )
            {
                var sourceProperties = type.GetProperties( bindingFlags )
                    .Select( property => property );

                if( type.IsInterface )
                {
                    sourceProperties = sourceProperties.Concat( type.GetInterfaces()
                        .SelectMany( i => i.GetProperties( bindingFlags ) ) );
                }

                sourceProperties = sourceProperties.Where(
                    p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in sourceProperties ) yield return property;
            }

            if( !this.IgnoreMethods )
            {
                var sourceMethods = type.GetMethods( bindingFlags )
                    .Select( method => method );

                if( type.IsInterface )
                {
                    sourceMethods = sourceMethods.Concat( type.GetInterfaces()
                        .SelectMany( i => i.GetMethods( bindingFlags ) ) );
                }

                if( this.AllowGetterMethodsOnly )
                    sourceMethods = sourceMethods.Where( method => method.IsGetterMethod() );

                foreach( var method in sourceMethods ) yield return method;
            }
        }
    }
}

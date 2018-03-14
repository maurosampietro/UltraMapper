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
        public bool IgnoreMethods { get; set; } = true;
        public bool IgnoreNonPublicMembers { get; set; } = true;

        IEnumerable<MemberInfo> IMemberProvider.GetMembers( Type type )
        {
            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            if( !this.IgnoreNonPublicMembers ) bindingAttributes |= BindingFlags.NonPublic;

            if( !this.IgnoreFields )
            {
                //- In case of an interface type we do nothing special since interfaces do not support fields
                //- Notice that we don't check for readonly fields: we only need to read from the source!

                var sourceFields = type.GetFields( bindingAttributes )
                    .Select( field => field );

                foreach( var field in sourceFields ) yield return field;
            }

            if( !this.IgnoreProperties )
            {
                var sourceProperties = type.GetProperties( bindingAttributes )
                    .Select( property => property );

                if( type.IsInterface )
                {
                    sourceProperties = sourceProperties.Concat( type.GetInterfaces()
                        .SelectMany( i => i.GetProperties( bindingAttributes ) ) );
                }

                sourceProperties = sourceProperties.Where(
                    p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in sourceProperties ) yield return property;
            }

            if( !this.IgnoreMethods )
            {
                var sourceMethods = type.GetMethods( bindingAttributes )
                    .Select( method => method );

                if( type.IsInterface )
                {
                    sourceMethods = sourceMethods.Concat( type.GetInterfaces()
                        .SelectMany( i => i.GetMethods( bindingAttributes ) ) );
                }

                sourceMethods = sourceMethods.Where( method => method.IsGetterMethod() );

                foreach( var method in sourceMethods ) yield return method;
            }
        }
    }
}

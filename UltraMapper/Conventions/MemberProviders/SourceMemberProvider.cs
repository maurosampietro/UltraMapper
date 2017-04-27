using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class SourceMemberProvider : IMemberProvider
    {
        public bool IgnoreFields { get; set; } = true;
        public bool IgnoreProperties { get; set; } = false;
        public bool IgnoreMethods { get; set; } = false;
        public bool IgnoreNonPublicMembers { get; set; } = true;

        IEnumerable<MemberInfo> IMemberProvider.GetMembers( Type type )
        {
            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.IgnoreNonPublicMembers ) bindingAttributes |= BindingFlags.NonPublic;

            if( !this.IgnoreFields )
            {
                //Notice that we don't check for readonly fields: we only need to read from the source!

                var sourceFields = type.GetFields( bindingAttributes )
                    .Select( field => field );

                foreach( var field in sourceFields ) yield return field;
            }

            if( !this.IgnoreProperties )
            {
                var sourceProperties = type.GetProperties( bindingAttributes )
                    .Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in sourceProperties ) yield return property;
            }

            if( !this.IgnoreMethods )
            {
                var sourceMethods = type.GetMethods( bindingAttributes )
                    .Where( method => method.IsGetterMethod() );

                foreach( var method in sourceMethods ) yield return method;
            }
        }
    }
}

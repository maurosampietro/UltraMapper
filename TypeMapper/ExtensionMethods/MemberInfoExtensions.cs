using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper
{
    public static class MemberInfoExtensions
    {
        public static Type GetMemberType( this MemberInfo memberInfo )
        {
            var type = (memberInfo as FieldInfo)?.FieldType;
            if( type != null ) return type;

            type = (memberInfo as PropertyInfo)?.PropertyType;
            if( type != null ) return type;

            throw new ArgumentException( $"Cannot handle {memberInfo}" );
        }

        public static object GetValue( this MemberInfo memberInfo, object source )
        {
            var field = (memberInfo as FieldInfo);
            if( field != null ) return field.GetValue( source );

            var property = (memberInfo as PropertyInfo);
            if( property != null ) return property.GetValue( source );

            throw new ArgumentException( $"Cannot handle {memberInfo}" );
        }
    }
}

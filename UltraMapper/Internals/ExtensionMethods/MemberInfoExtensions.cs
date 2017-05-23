using System;
using System.Reflection;

namespace UltraMapper.Internals
{
    internal static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets the type of the accessed member (last member) of the expression.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static Type GetMemberType( this MemberInfo memberInfo )
        {
            var type = memberInfo as Type;
            if( type != null ) return type;

            var field = memberInfo as FieldInfo;
            if( field != null ) return field.FieldType;

            var property = memberInfo as PropertyInfo;
            if( property != null ) return property.PropertyType;

            var method = memberInfo as MethodInfo;
            if( method != null )
            {
                if( method.IsGetterMethod() )
                    return method.ReturnType;

                if( method.IsSetterMethod() )
                    return method.GetParameters()[ 0 ].ParameterType;

                throw new ArgumentException( "Only methods in the form of (T)Get_Value() " +
                    "or (void)Set_Value(T value) are supported." );
            }

            throw new ArgumentException( $"'{memberInfo}' is not supported." );
        }
    }
}

using System;
using System.Reflection;

namespace UltraMapper.Internals
{
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets the type of the accessed member (last member) of the expression.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static Type GetMemberType( this MemberInfo memberInfo )
        {
            switch( memberInfo )
            {
                case Type type: return type;
                case FieldInfo field: return field.FieldType;
                case PropertyInfo property: return property.PropertyType;

                case MethodInfo method:
                {
                    if( method.IsGetterMethod() )
                        return method.ReturnType;

                    if( method.IsSetterMethod() )
                        return method.GetParameters()[ 0 ].ParameterType;

                    throw new ArgumentException( "Only methods in the form of (T)Get_Value() " +
                        "or (void)Set_Value(T value) are supported." );
                }
            }

            throw new ArgumentException( $"'{memberInfo}' is not supported." );
        }

        /// <summary>
        /// Gets the type of the accessed member (last member) of the expression.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        public static bool TryGetMemberType( this MemberInfo memberInfo, out Type memberType )
        {
            memberType = null;

            switch( memberInfo )
            {
                case Type type:
                {
                    memberType = type;
                    return true;
                }

                case FieldInfo field:
                {
                    memberType = field.FieldType;
                    return true;
                }

                case PropertyInfo property:
                {
                    memberType = property.PropertyType;
                    return true;
                }

                case MethodInfo methodInfo:
                {
                    if( methodInfo.IsGetterMethod() )
                        memberType = methodInfo.ReturnType;

                    if( methodInfo.IsSetterMethod() )
                        memberType = methodInfo.GetParameters()[ 0 ].ParameterType;

                    return false;
                }
            }

            return false;
        }
    }
}

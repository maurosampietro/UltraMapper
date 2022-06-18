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
        /// Attemps to get the type of the accessed member (last member) of the expression.
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="memberType">  When this method returns, contains the accessed member if found; 
        /// otherwise, the default value for the type of the value parameter
        /// <returns></returns>
        public static bool TryGetMemberType( this MemberInfo memberInfo, out Type memberType )
        {
            memberType = default;
            
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

        /// <summary>
        /// Checks if a member it's part of a specific type.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static bool BelongsTo<TTarget>( this MemberInfo member )
            => BelongsTo( member, typeof( TTarget ) );

        /// <summary>
        /// Checks if a member it's part of a specific type.
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static bool BelongsTo( this MemberInfo member, Type type )
            => type == member.ReflectedType;
    }
}

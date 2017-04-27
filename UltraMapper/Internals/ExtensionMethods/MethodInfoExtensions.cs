using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals
{
    internal static class MethodInfoExtensions
    {
        /// <summary>
        /// Checks if a method is a getter.
        /// </summary>
        /// <param name="methodInfo">The method to be inspected</param>
        /// <returns>True if a method is parameterless and its return type is not void; False otherwise</returns>
        public static bool IsGetterMethod( this MethodInfo methodInfo )
        {
            return methodInfo.ReturnType != typeof( void ) &&
                methodInfo.GetParameters().Length == 0;
        }

        /// <summary>
        /// Checks if a method is a getter of a given return type.
        /// </summary>
        /// <typeparam name="T">Expected return type of the method</typeparam>
        /// <param name="methodInfo">The method to be inspected</param>
        /// <returns>True if a method is parameterless and its return type is of type <typeparamref name="T"/>; False otherwise</returns>
        public static bool IsGetterMethod<T>( this MethodInfo methodInfo )
        {
            return IsGetterMethod( methodInfo, typeof( T ) );
        }

        /// <summary>
        /// Checks if a method is a getter of a given return type.
        /// </summary>
        /// <param name="methodInfo">The method to be inspected</param>
        /// <param name="returnType">Expected return type of the method</param>
        /// <returns>True if a method is parameterless and its return type is of type <paramref name="returnType"/> ; False otherwise</returns>
        public static bool IsGetterMethod( this MethodInfo methodInfo, Type returnType )
        {
            return methodInfo.ReturnType == returnType &&
                methodInfo.GetParameters().Length == 0;
        }

        /// <summary>
        /// Checks if a method is a setter.
        /// </summary>
        /// <param name="methodInfo">The method to be inspected</param>
        /// <returns>True if a method returns void and takes as input exactly one parameter; False otherwise</returns>
        public static bool IsSetterMethod( this MethodInfo methodInfo )
        {
            return methodInfo.ReturnType == typeof( void ) &&
                methodInfo.GetParameters().Length == 1;
        }

        /// <summary>
        /// Checks if a method is a setter taking as input exactly one parameter of a given type.
        /// </summary>
        /// <typeparam name="T">Expected type of the input parameter</typeparam>
        /// <param name="methodInfo">The method to be inspected</param>
        /// <returns>True if a method returns void and takes as input exactly one parameter of type <typeparamref name="T"/>; False otherwise</returns>
        public static bool IsSetterMethod<T>( this MethodInfo methodInfo )
        {
            return IsSetterMethod( methodInfo, typeof( T ) );
        }

        /// <summary>
        /// Checks if a method is a setter taking as input exactly one parameter of a given type.
        /// </summary>
        /// <param name="methodInfo">The method to be inspected</param>
        /// <param name="paramType">Expected type of the input parameter</param>
        /// <returns>True if a method returns void and takes as input exactly one parameter of type <paramref name="paramType"/>; False otherwise</returns>
        public static bool IsSetterMethod( this MethodInfo methodInfo, Type paramType )
        {
            var parameters = methodInfo.GetParameters();

            return methodInfo.ReturnType == typeof( void ) && parameters.Length == 1 
                && parameters[ 0 ].ParameterType == paramType;
        }
    }
}

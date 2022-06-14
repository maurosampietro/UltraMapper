using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class TargetMemberProvider : ITargetMemberProvider
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
        /// Consider only methods that return void and take as input exactly one parameter (getters or getter-like methods); 
        /// Or methods that return anything but void and take exactly one parameter as input (setters or setters-like methods).
        /// </summary>
        public bool AllowGetterOrSetterMethodsOnly { get; set; } = true;

        /// <summary>
        /// Allows to write on readonly fields and to set getter only properties 
        /// (since the compiler generates readonly backingfields for getter only properties).
        /// 
        /// ExpressionTrees, unlike reflection, enforce C# language and thus does not allow to write on readonly fields. 
        /// https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/2727812-allow-expression-assign-to-set-readonly-struct-f
        /// 
        /// This option is ready in case this is fixed by Microsoft.
        /// </summary>
        public readonly bool IgnoreReadonlyFields = true;

        public IEnumerable<MemberInfo> GetMembers( Type type )
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if( this.FlattenHierarchy ) bindingFlags |= BindingFlags.FlattenHierarchy;
            if( this.DeclaredOnly ) bindingFlags |= BindingFlags.DeclaredOnly;

            if( !this.IgnoreNonPublicMembers ) bindingFlags |= BindingFlags.NonPublic;

            if( !this.IgnoreFields )
            {
                //- In case of an interface type we do nothing special since interfaces do not support fields

                var targetFields = type.GetFields( bindingFlags )
                  .Select( field => field );

                if( IgnoreReadonlyFields )
                    targetFields = targetFields.Where( field => !field.IsInitOnly );

                foreach( var field in targetFields )
                    yield return field;
            }

            if( !this.IgnoreProperties )
            {
                var targetProperties = type.GetProperties( bindingFlags )
                    .Select( property => property );

                if( type.IsInterface )
                {
                    targetProperties = targetProperties.Concat( type.GetInterfaces()
                        .SelectMany( i => i.GetProperties( bindingFlags ) ) );
                }

                targetProperties = targetProperties.Where( property =>
                     property.CanWrite && property.GetSetMethod() != null
                     && property.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in targetProperties )
                    yield return property;
            }

            if( !this.IgnoreMethods )
            {
                var targetMethods = type.GetMethods( bindingFlags )
                    .Select( method => method );

                if( type.IsInterface )
                {
                    targetMethods = targetMethods.Concat( type.GetInterfaces()
                        .SelectMany( i => i.GetMethods( bindingFlags ) ) );
                }

                if( this.AllowGetterOrSetterMethodsOnly )
                {
                    targetMethods = targetMethods.Where( method =>
                        method.IsGetterMethod() || method.IsSetterMethod() );
                }

                foreach( var method in targetMethods )
                    yield return method;
            }
        }
    }
}

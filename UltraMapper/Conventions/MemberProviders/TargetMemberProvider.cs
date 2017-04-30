﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class TargetMemberProvider : IMemberProvider
    {
        public bool IgnoreFields { get; set; } = true;
        public bool IgnoreProperties { get; set; } = false;
        public bool IgnoreMethods { get; set; } = false;
        public bool IgnoreNonPublicMembers { get; set; } = true;

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
            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.IgnoreNonPublicMembers ) bindingAttributes |= BindingFlags.NonPublic;

            if( !this.IgnoreFields )
            {
                var targetFields = type.GetFields( bindingAttributes )
                  .Select( field => field );

                if( this.IgnoreReadonlyFields )
                    targetFields = targetFields.Where( field => !field.IsInitOnly );

                foreach( var field in targetFields ) yield return field;
            }

            if( !this.IgnoreProperties )
            {
                var targetProperties = type.GetProperties( bindingAttributes )
                    .Where( p => p.CanWrite && p.GetSetMethod() != null &&
                    p.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in targetProperties ) yield return property;
            }

            if( !this.IgnoreMethods )
            {
                var targetMethods = type.GetMethods( bindingAttributes )
                    .Where( method => method.IsSetterMethod() );

                foreach( var method in targetMethods ) yield return method;
            }
        }
    }
}
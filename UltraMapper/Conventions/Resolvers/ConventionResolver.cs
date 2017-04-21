using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;
using UltraMapper.Conventions;

namespace UltraMapper.Conventions
{
    public class ConventionResolver : IConventionResolver
    {
        public readonly IMappingConvention MappingConvention;

        public bool IgnoreSourceFields { get; set; } = true;
        public bool IgnoreSourceProperties { get; set; } = false;
        public bool IgnoreSourceGetMethods { get; set; } = true;
        public bool IgnoreSourceNonPublicMembers { get; set; } = true;

        public bool IgnoreTargetFields { get; set; } = true;
        public bool IgnoreTargetProperties { get; set; } = false;
        public bool IgnoreTargetSetMethods { get; set; } = true;
        public bool IgnoreTargetNonPublicMembers { get; set; } = true;

        /// <summary>
        /// Allows to write on readonly fields and to set getter only properties 
        /// (since the compiler generates readonly backingfields for getter only properties).
        /// 
        /// ExpressionTrees, unlike reflection, enforce C# language and thus does not allow to write on readonly fields. 
        /// https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/2727812-allow-expression-assign-to-set-readonly-struct-f
        /// 
        /// This option is ready in case this is fixed by Microsoft.
        /// </summary>
        public readonly bool IgnoreTargetReadonlyFields = true;

        public ConventionResolver( IMappingConvention mappingConvention )
        {
            this.MappingConvention = mappingConvention;
        }

        public IEnumerable<MemberPair> Resolve( Type source, Type target )
        {
            var sourceMembers = this.GetSourceMembers( source );
            var targetMembers = this.GetTargetMembers( target ).ToList();

            foreach( var sourceMember in sourceMembers )
            {
                foreach( var targetMember in targetMembers )
                {
                    if( this.MappingConvention.IsMatch( sourceMember, targetMember ) )
                    {
                        yield return new MemberPair( sourceMember, targetMember );
                        break; //sourceMember is now mapped, jump directly to the next sourceMember
                    }
                }
            }
        }

        private IEnumerable<MemberInfo> GetSourceMembers( Type source )
        {
            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.IgnoreSourceNonPublicMembers ) bindingAttributes |= BindingFlags.NonPublic;

            if( !this.IgnoreSourceFields )
            {
                //Notice that we don't check for readonly fields: we only need to read from the source!

                var sourceFields = source.GetFields( bindingAttributes )
                    .Select( field => field );

                foreach( var field in sourceFields ) yield return field;
            }

            if( !this.IgnoreSourceProperties )
            {
                var sourceProperties = source.GetProperties( bindingAttributes )
                    .Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in sourceProperties ) yield return property;
            }

            if( !this.IgnoreSourceGetMethods )
            {
                Func<MethodInfo, bool> isGetterMethod = method =>
                {
                    bool isGetter = method.Name.StartsWith( "Get", StringComparison.InvariantCultureIgnoreCase );
                    bool isVoid = method.ReturnType == typeof( void );
                    bool isParameterless = method.GetParameters().Length == 0;

                    return isGetter && isVoid && isParameterless;
                };

                var sourceMethods = source.GetMethods( bindingAttributes ).Where( isGetterMethod );
                foreach( var method in sourceMethods ) yield return method;
            }
        }

        private IEnumerable<MemberInfo> GetTargetMembers( Type target )
        {
            var bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            if( !this.IgnoreTargetNonPublicMembers ) bindingAttributes |= BindingFlags.NonPublic;

            if( !this.IgnoreTargetFields )
            {
                var targetFields = target.GetFields( bindingAttributes )
                  .Select( field => field );

                if( this.IgnoreTargetReadonlyFields )
                    targetFields = targetFields.Where( field => !field.IsInitOnly );

                foreach( var field in targetFields ) yield return field;
            }

            if( !this.IgnoreTargetProperties )
            {
                var targetProperties = target.GetProperties( bindingAttributes )
                    .Where( p => p.CanWrite && p.GetSetMethod() != null &&
                    p.GetIndexParameters().Length == 0 ); //no indexed properties

                foreach( var property in targetProperties ) yield return property;
            }

            if( !this.IgnoreTargetSetMethods )
            {
                Func<MethodInfo, bool> isSetterMethod = method =>
                {
                    bool isSetter = method.Name.StartsWith( "Set", StringComparison.InvariantCultureIgnoreCase );
                    bool isMonadic = method.GetParameters().Length == 1;

                    return isSetter && isMonadic;
                };

                var targetMethods = target.GetMethods( bindingAttributes ).Where( isSetterMethod );
                foreach( var method in targetMethods ) yield return method;
            }
        }
    }
}

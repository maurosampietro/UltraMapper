using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MethodMatching : INameMatchingRule
    {
        public bool IgnoreCase { get; set; }

        public string[] GetMethodPrefixes { get; set; }
        public string[] SetMethodPrefixes { get; set; }

        public MethodMatching()
        {
            this.GetMethodPrefixes = new string[] { "Get_", "Get" };
            this.SetMethodPrefixes = new string[] { "Set_", "Set" };
        }

        public bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            if( source is MethodInfo )
            {
                var methodInfo = (MethodInfo)source;
                if( !methodInfo.IsGetterMethod() ) return false;
            }

            if( target is MethodInfo )
            {
                var methodInfo = (MethodInfo)target;
                if( !methodInfo.IsSetterMethod() ) return false;
            }

            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var sourceName = this.CleanAffix( source.Name, this.GetMethodPrefixes );
            var targetName = this.CleanAffix( target.Name, this.SetMethodPrefixes );

            return sourceName.Equals( targetName, comparisonType );
        }

        private string CleanAffix( string input, string[] affixes )
        {
            foreach( var affix in affixes )
            {
                if( input.StartsWith( affix, this.IgnoreCase, CultureInfo.InvariantCulture ) )
                    return input.Remove( 0, affix.Length );
            }

            return input;
        }
    }
}

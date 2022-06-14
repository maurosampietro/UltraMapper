using System;
using System.Globalization;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MethodNameMatching : INameMatchingRule
    {
        public bool IgnoreCase { get; set; }

        public string[] GetMethodPrefixes { get; set; }
        public string[] SetMethodPrefixes { get; set; }

        public MethodNameMatching()
        {
            this.GetMethodPrefixes = new string[] { "Get_", "Get" };
            this.SetMethodPrefixes = new string[] { "Set_", "Set" };
        }

        public bool CanHandle( MemberInfo source, MemberInfo target )
        {
            return source is MethodInfo && target is MethodInfo;
        }

        public bool IsCompliant( MemberInfo source, MemberInfo target )
        {
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

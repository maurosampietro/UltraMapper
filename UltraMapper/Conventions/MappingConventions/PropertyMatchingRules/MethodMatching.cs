using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Conventions.MappingConventions.PropertyMatchingRules
{
    public class MethodMatching : MatchingRuleBase
    {
        public bool IgnoreCase { get; set; }

        public string[] GetMethodPrefixes { get; set; }
        public string[] SetMethodPrefixes { get; set; }

        public MethodMatching()
        {
            this.GetMethodPrefixes = new string[] { "Get_", "Get" };
            this.SetMethodPrefixes = new string[] { "Set_", "Set" };
        }

        public override bool IsCompliant( MemberInfo source, MemberInfo target )
        {
            var comparisonType = this.IgnoreCase ?
                StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var sourceName = source.Name;
            foreach( var prefix in GetMethodPrefixes )
            {
                if( source.Name.StartsWith( prefix ) )
                {
                    sourceName = source.Name.Remove( 0, prefix.Length );
                    break;
                }
            }

            var targetName = target.Name;
            foreach( var prefix in SetMethodPrefixes )
            {
                if( target.Name.StartsWith( prefix ) )
                {
                    targetName = target.Name.Remove( 0, prefix.Length );
                    break;
                }
            }

            //return new ExactNameMatching().IsCompliant()
            return this.GetMethodPrefixes.Any( getPrefix =>
                target.Name.Equals( source.Name + getPrefix, comparisonType ) );
        }
    }
}

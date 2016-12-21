using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.MappingConventions;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Two properties match if they have the same name and the type
    /// is the same or implicitly convertible to the target type.
    /// </summary>
    public class DefaultMappingConvention : IMappingConvention
    {
        public PropertyMatchingConfiguration PropertyMatchingRules { get; set; }

        public DefaultMappingConvention()
        {
            this.PropertyMatchingRules = new PropertyMatchingConfiguration( cfg =>
            {
                cfg.GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = false );
                cfg.GetOrAdd<TypeMatchingRule>( rule => rule.AllowImplicitConversions = true );
            } );
        }

        public bool IsMatch( PropertyInfo source, PropertyInfo target )
        {
            return this.PropertyMatchingRules.MatchingEvaluator( source, target );
        }
    }
}

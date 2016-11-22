using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions.PropertyMatchingRules
{
    /// <summary>
    /// Two properties match if the source type is of the same type 
    /// or (optionally) implicitly convertible to the destination type.
    /// </summary>
    public class TypeMatchingRule : PropertyMatchingRuleBase
    {
        public bool AllowImplicitConversions { get; set; } = true;

        public override bool IsCompliant( PropertyInfo source, PropertyInfo destination )
        {
            var sourceType = source.PropertyType;
            var destinationType = destination.PropertyType;

            return source.PropertyType == destination.PropertyType || (this.AllowImplicitConversions 
                && sourceType.IsImplicitlyConvertibleTo( destinationType ));
        }
    }
}

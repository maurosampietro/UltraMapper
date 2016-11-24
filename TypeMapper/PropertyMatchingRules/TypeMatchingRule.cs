using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    /// <summary>
    /// Two properties match if the source type is of the same type 
    /// or (optionally) implicitly convertible to the target type.
    /// </summary>
    public class TypeMatchingRule : PropertyMatchingRuleBase
    {
        public bool AllowImplicitConversions { get; set; } = true;

        public override bool IsCompliant( PropertyInfo source, PropertyInfo target )
        {
            var sourceType = source.PropertyType;
            var targetType = target.PropertyType;

            return source.PropertyType == target.PropertyType || (this.AllowImplicitConversions 
                && sourceType.IsImplicitlyConvertibleTo( targetType ));
        }
    }
}

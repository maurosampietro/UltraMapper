using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class TargetProperty<TTarget> : PropertyBase
    {
        //This info is evaluated at configuration level only once for performance reasons
        public Type NullableUnderlyingType { get; set; }
        public Action<TTarget, object> ValueSetter { get; set; }

        public TargetProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }

    public class TargetProperty : TargetProperty<object>
    {
        public TargetProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }
}

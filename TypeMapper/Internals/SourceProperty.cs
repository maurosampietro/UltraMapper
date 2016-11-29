using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class SourceProperty : SourceProperty<object>
    {
        public SourceProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }

    public class SourceProperty<TSource> : PropertyBase
    {
        //This info is evaluated at configuration level only once for performance reasons
        public bool IsEnumerable { get; set; }
        public Func<TSource, object> ValueGetter { get; set; }

        public SourceProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class SourceProperty : PropertyBase
    {
        //This info is evaluated at configuration level only once for performance reasons
        public bool IsEnumerable { get; set; }

        public Expression ValueGetterExpr { get; set; }

        public SourceProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }
}

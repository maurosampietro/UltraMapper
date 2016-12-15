using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.CollectionMappingStrategies;

namespace TypeMapper.Internals
{
    public class TargetProperty : PropertyBase
    {
        //This info is evaluated at configuration level only once for performance reasons
        public Type NullableUnderlyingType { get; set; }

        public LambdaExpression ValueSetter { get; set; }
        public LambdaExpression ValueGetter { get; set; }

        public ICollectionMappingStrategy CollectionStrategy { get; set; }

        public TargetProperty( PropertyInfo propertyInfo )
            : base( propertyInfo )
        {
            this.CollectionStrategy = new NewCollection();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.CollectionMappingStrategies;

namespace TypeMapper.Internals
{
    public class TargetProperty<TTarget> : PropertyBase
    {
        //This info is evaluated at configuration level only once for performance reasons
        public Type NullableUnderlyingType { get; set; }
        public Action<TTarget, object> ValueSetter { get; set; }
        public ICollectionMappingStrategy CollectionStrategy { get; set; }

        private Func<TTarget> _instanceCreator;

        public TargetProperty( PropertyInfo propertyInfo )
            : base( propertyInfo )
        {
            _instanceCreator = ConstructorFactory.GetOrCreateConstructor<TTarget>();
            this.CollectionStrategy = new KeepCollection();
        }

        public TTarget GetDefaultValue()
        {
            return _instanceCreator();
        }
    }

    public class TargetProperty : TargetProperty<object>
    {
        public TargetProperty( PropertyInfo propertyInfo )
            : base( propertyInfo ) { }
    }
}

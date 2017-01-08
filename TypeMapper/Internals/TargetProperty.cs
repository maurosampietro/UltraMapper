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
        public LambdaExpression ValueSetter { get; set; }
        public LambdaExpression ValueGetter { get; set; }

        public LambdaExpression CustomConstructor { get; set; }
        public ICollectionMappingStrategy CollectionStrategy { get; set; }

        internal TargetProperty( PropertyInfo propertySelector )
            : base( propertySelector )
        {
            this.CollectionStrategy = new NewCollection();

            this.ValueSetter = base.PropertyInfo.GetSetterLambdaExpression();
            this.ValueGetter = base.PropertyInfo.GetGetterLambdaExpression();
        }
    }
}

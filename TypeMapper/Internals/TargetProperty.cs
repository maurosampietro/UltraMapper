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
        //Each source proeprty can be instantiated only once
        //so we can handle source property options.
        private static readonly Dictionary<PropertyInfo, TargetProperty> _cachedProperties
            = new Dictionary<PropertyInfo, TargetProperty>();

        public LambdaExpression ValueSetter { get; set; }
        public LambdaExpression ValueGetter { get; set; }

        public LambdaExpression CustomConstructor { get; set; }
        public ICollectionMappingStrategy CollectionStrategy { get; set; }

        public TargetProperty( PropertyInfo propertySelector )
            : base( propertySelector )
        {
            this.CollectionStrategy = new NewCollection();

            this.ValueSetter = base.PropertyInfo.GetSetterLambdaExpression();
            this.ValueGetter = base.PropertyInfo.GetGetterLambdaExpression();
        }

        public static TargetProperty GetTargetProperty( PropertyInfo propertyInfo )
        {
            TargetProperty targetProperty;
            if( !_cachedProperties.TryGetValue( propertyInfo, out targetProperty ) )
            {
                targetProperty = new TargetProperty( propertyInfo );
                _cachedProperties.Add( propertyInfo, targetProperty );
            }

            return targetProperty;
        }
    }
}

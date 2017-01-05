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
        //Each source proeprty can be instantiated only once
        //so we can handle source property options.
        private static readonly Dictionary<PropertyInfo, SourceProperty> _cachedProperties
            = new Dictionary<PropertyInfo, SourceProperty>();

        public LambdaExpression ValueGetter { get; set; }

        private SourceProperty( PropertyInfo propertyInfo )
            : base( propertyInfo )
        {
            //((MemberExpression)propertySelector.Body).Member
            this.ValueGetter = base.PropertyInfo.GetGetterLambdaExpression();
        }

        public static SourceProperty GetSourceProperty( PropertyInfo propertyInfo )
        {
            SourceProperty sourceProperty;
            if( !_cachedProperties.TryGetValue( propertyInfo, out sourceProperty ) )
            {
                sourceProperty = new SourceProperty( propertyInfo );
                _cachedProperties.Add( propertyInfo, sourceProperty );
            }

            return sourceProperty;
        }
    }
}

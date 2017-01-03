using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class PropertyBase
    {
        //public readonly LambdaExpression PropertySelector;
        public readonly PropertyInfo PropertyInfo;
        private readonly Lazy<string> _toString;

        //These info are evaluated at configuration level only once for performance reasons
        public bool IsEnumerable { get; set; }
        public bool IsBuiltInType { get; set; }
        public bool IsNullable { get; set; }
        public Type NullableUnderlyingType { get; set; }

        public PropertyBase( PropertyInfo propertyInfo )
        {
            //this.PropertySelector = propertySelector;

            this.PropertyInfo = propertyInfo;

            this.NullableUnderlyingType = Nullable.GetUnderlyingType( propertyInfo.PropertyType );
            this.IsBuiltInType = propertyInfo.PropertyType.IsBuiltInType( false );
            this.IsNullable = this.NullableUnderlyingType != null;
            this.IsEnumerable = propertyInfo.PropertyType.IsEnumerable();

            _toString = new Lazy<string>( () =>
            {
                string typeName = propertyInfo.PropertyType.GetPrettifiedName();
                return $"{typeName} {propertyInfo.Name}";
            } );
        }

        public override bool Equals( object obj )
        {
            var propertyBase = obj as PropertyBase;
            if( propertyBase == null ) return false;

            return this.PropertyInfo.Equals( propertyBase.PropertyInfo );
        }

        public override int GetHashCode()
        {
            return this.PropertyInfo.GetHashCode();
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

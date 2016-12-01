using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class PropertyBase
    {
        public readonly PropertyInfo PropertyInfo;
        private readonly Lazy<string> _toString;

        public bool IsBuiltInType { get; set; }

        public PropertyBase( PropertyInfo propertyInfo )
        {
            this.PropertyInfo = propertyInfo;

            _toString = new Lazy<string>( () =>
            {
                string typeName = propertyInfo.PropertyType.GetPrettifiedName();
                return $"{typeName} {propertyInfo.Name}";
            } );
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as SourceProperty;
            if( typePair == null ) return false;

            return this.PropertyInfo.Equals( typePair.PropertyInfo );
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

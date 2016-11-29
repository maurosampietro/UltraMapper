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
        public bool IsBuiltInType { get; set; }

        public PropertyBase( PropertyInfo propertyInfo )
        {
            this.PropertyInfo = propertyInfo;
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    internal class PropertyInfoPair
    {
        public readonly PropertyInfo SourceProperty;
        public readonly PropertyInfo TargetProperty;

        private readonly int _hashcode;
        private readonly Lazy<string> _toString;

        public PropertyInfoPair( PropertyInfo sourceType, PropertyInfo targetType )
        {
            this.SourceProperty = sourceType;
            this.TargetProperty = targetType;

            _hashcode = unchecked(this.SourceProperty.GetHashCode() * 31)
                ^ this.TargetProperty.GetHashCode();

            _toString = new Lazy<string>( () =>
            {
                string sourceName = this.SourceProperty.Name;
                string sourceTypeName = this.SourceProperty.PropertyType.GetPrettifiedName();

                string targetName = this.TargetProperty.Name;
                string targetTypeName = this.TargetProperty.PropertyType.GetPrettifiedName();

                return $"[{sourceTypeName} {sourceName}, {targetTypeName} {targetName}]";
            } );
        }

        public override bool Equals( object obj )
        {
            var typePair = obj as PropertyInfoPair;
            if( typePair == null ) return false;

            return this.SourceProperty.Equals( typePair.SourceProperty ) &&
                this.TargetProperty.Equals( typePair.TargetProperty );
        }

        public override int GetHashCode()
        {
            return _hashcode;
        }

        public override string ToString()
        {
            return _toString.Value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    public class SameNameAndTypeConvention : IMappingConvention
    {
        public bool IsMatch( PropertyInfo source, PropertyInfo destination )
        {
            return source.PropertyType == destination.PropertyType
                && source.Name == destination.Name;
        }
    }
}

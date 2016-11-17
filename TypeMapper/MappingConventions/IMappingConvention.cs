using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.MappingConventions
{
    public interface IMappingConvention
    {
        bool IsMatch( PropertyInfo source, PropertyInfo destination );
    }
}

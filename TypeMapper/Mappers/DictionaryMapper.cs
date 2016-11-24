using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public class DictionaryMapper
    {
        public object CreateNewInstance( Type type, params object[] values )
        {
            return InstanceFactory.CreateObject( type, values );
        }
    }
}

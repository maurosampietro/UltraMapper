using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Internals
{
    public class ObjectPair
    {
        public readonly object Source;
        public readonly object Target;

        public ObjectPair( object source, object target )
        {
            this.Source = source;
            this.Target = target;
        }
    }
}

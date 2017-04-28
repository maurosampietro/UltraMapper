using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MappingConventions : SingletonList<IMappingConvention>
    {
        public MappingConventions( Action<SingletonList<IMappingConvention>> config = null )
            : base( config ) { }
    }
}

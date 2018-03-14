using System;
using UltraMapper.Internals;

namespace UltraMapper.Conventions
{
    public class MappingConventions : SingletonList<IMappingConvention>
    {
        public MappingConventions( Action<SingletonList<IMappingConvention>> config = null )
            : base( config ) { }
    }
}

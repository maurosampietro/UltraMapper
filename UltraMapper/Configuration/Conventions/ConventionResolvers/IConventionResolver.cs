using System.Collections.Generic;
using UltraMapper.Internals;

namespace UltraMapper.Conventions.Resolvers
{
    interface IConventionResolver
    {
        void MapByConvention( TypeMapping newTypeMapping, 
            IEnumerable<IMappingConvention> conventions );
    }
}

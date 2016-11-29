using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;

namespace TypeMapper.Mappers
{
    public interface IObjectMapper
    {
        /// <summary>
        /// Gets a value indicating whether the mapper can handle <paramref name="mapping"/>
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns>True if the mapping can be handled by the mapper, False otherwise.</returns>
        bool CanHandle( PropertyMapping mapping );

        /// <summary>
        /// Map <paramref name"value"/> to the corresponding object in <paramref name="targetInstance"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetInstance"></param>
        /// <param name="mapping"></param>
        /// <param name="referenceTracking"></param>
        /// <returns>Returns a list of objects that need to be recursively mapped</returns>
        IEnumerable<ObjectPair> Map( object value, object targetInstance,
            PropertyMapping mapping, IReferenceTracking referenceTracking );
    }
}

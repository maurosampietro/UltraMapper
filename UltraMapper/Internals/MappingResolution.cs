using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Internals
{
    public enum MappingResolution
    {
        /// <summary>
        /// Indicates that a mapping has been resolved 
        /// automatically via conventions.
        /// </summary>
        RESOLVED_BY_CONVENTION,

        /// <summary>
        /// Indicates that a mapping has been 
        /// configured manually by the user
        /// </summary>
        USER_DEFINED
    }
}

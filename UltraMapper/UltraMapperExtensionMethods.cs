using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper
{
    public static class UltraMapperExtensionMethods
    {
        private static Configuration _configuration = new Configuration();
        private static Mapper _mapper = new Mapper( _configuration );

        public static void MapTo<TSource, TTarget>( this TSource source,
            TTarget target, Action<Configuration> config = null ) where TTarget : class
        {
            config?.Invoke( _configuration );
            _mapper.Map( source, target );
        }
    }
}

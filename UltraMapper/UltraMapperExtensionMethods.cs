using System;

namespace UltraMapper
{
    public static class UltraMapperExtensionMethods
    {
        private static readonly Configuration _configuration = new Configuration();
        private static Mapper _mapper = new Mapper( _configuration );

        public static void MapTo<TSource, TTarget>( this TSource source,
            TTarget target, Action<Configuration> config = null ) where TTarget : class
        {
            config?.Invoke( _configuration );
            _mapper.Map( source, target );
        }
    }
}

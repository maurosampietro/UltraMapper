using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Mappers;

namespace TypeMapper.Configuration
{
    public class ObjectMapperConfiguration : IEnumerable<IObjectMapper>
    {
        private ConcurrentDictionary<Type, IObjectMapper> _objectMappers =
            new ConcurrentDictionary<Type, IObjectMapper>();

        public ObjectMapperConfiguration Add<T>( T item ) where T : IObjectMapper
        {
            _objectMappers.AddOrUpdate( item.GetType(),
                item, ( type, instance ) => instance );

            return this;
        }

        public ObjectMapperConfiguration Add<T>() where T : IObjectMapper, new()
        {
            return this.Add( new T() );
        }

        public ObjectMapperConfiguration Remove<T>() where T : IObjectMapper
        {
            IObjectMapper instance;
            _objectMappers.TryRemove( typeof( T ), out instance );

            return this;
        }

        public IEnumerator<IObjectMapper> GetEnumerator()
        {
            return _objectMappers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

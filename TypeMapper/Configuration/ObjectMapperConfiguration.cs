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
        private class MapperComparer : IEqualityComparer<IObjectMapper>
        {
            public bool Equals( IObjectMapper x, IObjectMapper y )
            {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode( IObjectMapper obj )
            {
                return obj.GetType().GetHashCode();
            }
        }

        //it is mandatory to use a collection that preserves insertion order
        private HashSet<IObjectMapper> _objectMappers
             = new HashSet<IObjectMapper>( new MapperComparer() );

        public ObjectMapperConfiguration Add<T>( T item )
            where T : IObjectMapper
        {
            _objectMappers.Add( item );
            return this;
        }

        public ObjectMapperConfiguration Add<T>()
            where T : IObjectMapper, new()
        {
            return this.Add( new T() );
        }

        public ObjectMapperConfiguration Remove<T>()
            where T : IObjectMapper
        {
            var type = typeof( T );

            var mappersToRemove = _objectMappers.Where(
                mapper => mapper.GetType() == type );

            foreach( var mapper in mappersToRemove )
                _objectMappers.Remove( mapper );

            return this;
        }

        public IEnumerator<IObjectMapper> GetEnumerator()
        {
            return _objectMappers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

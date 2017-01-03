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
    public class ObjectMapperSet : IEnumerable<IObjectMapperExpression>
    {
        private class MapperComparer : IEqualityComparer<IObjectMapperExpression>
        {
            public bool Equals( IObjectMapperExpression x, IObjectMapperExpression y )
            {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode( IObjectMapperExpression obj )
            {
                return obj.GetType().GetHashCode();
            }
        }

        //it is mandatory to use a collection that preserves insertion order
        private HashSet<IObjectMapperExpression> _objectMappers
             = new HashSet<IObjectMapperExpression>( new MapperComparer() );

        public ObjectMapperSet Add<T>( T item, Action<T> config )
            where T : IObjectMapperExpression
        {
            _objectMappers.Add( item );
            config?.Invoke( item );

            return this;
        }

        public ObjectMapperSet Add<T>( Action<T> config = null )
            where T : IObjectMapperExpression, new()
        {
            return this.Add( new T(), config );
        }

        public ObjectMapperSet Remove<T>()
            where T : IObjectMapperExpression
        {
            var type = typeof( T );

            var mappersToRemove = _objectMappers.Where(
                mapper => mapper.GetType() == type );

            foreach( var mapper in mappersToRemove )
                _objectMappers.Remove( mapper );

            return this;
        }

        public IEnumerator<IObjectMapperExpression> GetEnumerator()
        {
            return _objectMappers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

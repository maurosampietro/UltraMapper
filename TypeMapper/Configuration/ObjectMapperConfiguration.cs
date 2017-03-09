using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TypeMapper.Mappers;

namespace TypeMapper.Configuration
{
    public class ObjectMapperSet : IEnumerable<IMemberMappingMapperExpression>
    {
        private class MapperComparer : IEqualityComparer<IMemberMappingMapperExpression>
        {
            public bool Equals( IMemberMappingMapperExpression x, IMemberMappingMapperExpression y )
            {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode( IMemberMappingMapperExpression obj )
            {
                return obj.GetType().GetHashCode();
            }
        }

        //it is mandatory to use a collection that preserves insertion order
        private HashSet<IMemberMappingMapperExpression> _objectMappers
             = new HashSet<IMemberMappingMapperExpression>( new MapperComparer() );

        public ObjectMapperSet Add<T>( T item, Action<T> config )
            where T : IMemberMappingMapperExpression
        {
            _objectMappers.Add( item );
            config?.Invoke( item );

            return this;
        }

        public ObjectMapperSet Add<T>( Action<T> config = null )
            where T : IMemberMappingMapperExpression, new()
        {
            return this.Add( new T(), config );
        }

        public ObjectMapperSet Remove<T>()
            where T : IMemberMappingMapperExpression
        {
            var type = typeof( T );

            var mappersToRemove = _objectMappers.Where(
                mapper => mapper.GetType() == type );

            foreach( var mapper in mappersToRemove )
                _objectMappers.Remove( mapper );

            return this;
        }

        public IEnumerator<IMemberMappingMapperExpression> GetEnumerator()
        {
            return _objectMappers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

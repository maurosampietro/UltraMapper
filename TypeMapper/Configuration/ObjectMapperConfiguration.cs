using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Mappers;

namespace TypeMapper.Configuration
{
    public class MappingExpressionBuilderFactory
    {
        private static HashSet<IMapperExpression> _mappers
            = new HashSet<IMapperExpression>()
        {
            //new CustomConverterMapper() ,
            new BuiltInTypeMapper()     ,
            new NullableMapper()        ,
            new ConvertMapper()         ,
            //new ReferenceMapper()       ,
            //new DictionaryMapper()      ,
            //new SetMapper()             ,
            //new StackMapper()           ,
            //new QueueMapper()           ,
            //new LinkedListMapper()      ,
            //new CollectionMapper()
        };

        //public static IMapperExpression GetExpressionBuilder( Type source, Type target )
        //{
        //    return _mappers.First( mapper =>
        //        mapper.CanHandle( source, target ) );
        //}

        public static LambdaExpression GetMappingExpression( Type source, Type target )
        {
            var selectedMapper = _mappers.FirstOrDefault( mapper =>
                mapper.CanHandle( source, target ) );

            if( selectedMapper == null )
                throw new Exception( $"No mapper can handle {source} -> {target}" );

            return selectedMapper.GetMappingExpression( source, target );
        }
    }

    internal class MapperComparer : IEqualityComparer<IObjectMapperExpression>
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

    public class ObjectMapperSet : IEnumerable<IObjectMapperExpression>
    {
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

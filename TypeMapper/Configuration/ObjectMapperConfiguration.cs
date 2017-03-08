using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TypeMapper.Internals;
using TypeMapper.Mappers;

namespace TypeMapper.Configuration
{
    public class MappingExpressionBuilderFactory
    {
        private static HashSet<ITypeMappingMapperExpression> _mappers
            = new HashSet<ITypeMappingMapperExpression>()
        {
            new CustomConverterMapper() ,
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
        //public static bool CanHandle( Type source, Type target )
        //{
        //    return _mappers.Any( mapper => mapper.CanHandle( source, target ) );
        //}

        public static LambdaExpression GetMappingExpression( Type source, Type target )
        {
            var selectedMapper = _mappers.FirstOrDefault( mapper =>
                mapper.CanHandle( source, target ) );

            if( selectedMapper == null )
                throw new Exception( $"No mapper can handle {source} -> {target}" );

            return selectedMapper.GetMappingExpression( source, target );
        }

        internal static bool CanHandle( TypeMapping typeMapping )
        {
            return _mappers.Any( mapper => mapper.CanHandle( typeMapping ) );
        }

        internal static LambdaExpression GetMappingExpression( TypeMapping typeMapping )
        {
            var selectedMapper = _mappers.FirstOrDefault( mapper =>
                          mapper.CanHandle( typeMapping ) );

            if( selectedMapper == null )
                throw new Exception( $"No mapper can handle {typeMapping}" );

            return selectedMapper.GetMappingExpression( typeMapping );
        }
    }

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

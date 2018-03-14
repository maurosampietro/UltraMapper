using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ArrayTests : CollectionTests
    {
        public class GenericArray<T>
        {
            public T[] Array { get; set; }
        }

        public class GenericCollection<T>
        {
            public ICollection<T> Collection { get; set; }
        }

        [TestMethod]
        public void SimpleArrayToCollection()
        {
            var source = new GenericArray<int>()
            {
                Array = Enumerable.Range( 0, 100 ).ToArray()
            };

            var target = new GenericCollections<int>( false );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.Array, b => b.List );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Array.SequenceEqual( target.List ) );
        }

        [TestMethod]
        public void SimpleCollectionToArray()
        {
            var source = new GenericCollections<int>( false )
            {
                List = Enumerable.Range( 0, 100 ).ToList()
            };

            var target = new GenericArray<int>();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.List, b => b.Array );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.List.SequenceEqual( target.Array ) );
        }

        [TestMethod]
        public void ComplexArrayToCollection()
        {
            var innerType = new InnerType() { String = "test" };
            var source = new GenericArray<ComplexType>()
            {
                Array = new ComplexType[ 3 ]
            };

            for( int i = 0; i < 3; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i, InnerType = innerType };
            }

            var target = new GenericCollections<ComplexType>( false );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.Array, b => b.List );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ComplexCollectionToArray()
        {
            var innerType = new InnerType() { String = "test" };
            var source = new GenericCollections<ComplexType>( false );

            for( int i = 0; i < 3; i++ )
            {
                source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
                source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
                source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
                source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
            }

            var target = new GenericArray<ComplexType>();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.List, b => b.Array );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ComplexArrayBehindInterface()
        {
            Expression<Func<int, int>> d = i => 1;

            var source = new GenericCollection<InnerType>()
            {
                Collection = new InnerType[ 2 ]
                {
                    new InnerType() { String = "A" },
                    new InnerType() { String = "B" }
                }
            };

            var target = new GenericCollection<InnerType>();

            var ultraMapper = new Mapper( cfg =>
            {
                //cfg.MapTypes( source, target )
                //   .MapMember( s => s.Collection, t => t.Collection, memberMappingConfig: config =>
                //   {
                //       Expression<Func<ICollection<InnerType>>> temp = () => new List<InnerType>();
                //       config.CustomTargetConstructor = temp;
                //   } );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class DifficultCollectionTests
    {

        [TestMethod]
        public void CollectionItemsAndMembersSelfMapping()
        {
            var source = new Test { Users = Enumerable.Range( 0, 10 ).Select( x => new User { Id = x } ).ToList() };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Test, Test>()
                 .MapMember( x => x.Users, y => y.Users,
                    cfg2 =>
                    {
                        cfg2.SetCustomTargetConstructor( () => new List<User>() );
                    }
                 );
                cfg.MapTypes<User, User>();
            } );
            var target = ultraMapper.Map<Test, Test>( source );
            Assert.AreEqual( target.Users.Count, 10 );
        }

        [TestMethod]
        public void CollectionItemsAndMembersMapping()
        {
            var source = new Test { Users = Enumerable.Range( 0, 10 ).Select( x => new User { Id = x } ).ToList() };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Test, TargetTest>()
                    .MapMember( s => s.Users, t => t.UsersTarget,
                    cfg2 =>
                    {
                        cfg2.SetCustomTargetConstructor( () => new List<TargetUser>() );
                    } );
                cfg.MapTypes<User, TargetUser>()
                    .MapMember( s => s.Id, t => t.IdTarget );
            } );
            var target = ultraMapper.Map<Test, TargetTest>( source );
            Assert.AreEqual( target.UsersTarget.Count, 10 );
        }


        public class User
        {
            public int Id { get; set; }
        }

        public class Test
        {
            public IList<User> Users { get; set; }
            public bool ShouldMap { get; set; }
        }

        public class TargetUser
        {
            public int IdTarget { get; set; }
        }

        public class TargetTest
        {
            public IList<TargetUser> UsersTarget { get; set; }
        }

        private class ComplicatedIList<T> : IList<T>
        {
            private List<T> _child;

            public static ComplicatedIList<T> MakeComplicatedList( IEnumerable<T> items )
            {
                return new ComplicatedIList<T>
                {
                    _child = items.ToList()
                };
            }

            public T this[ int index ] { get => _child[ index ]; set => throw new System.NotImplementedException(); }

            public int Count => _child.Count;

            public bool IsReadOnly => false;

            public void Add( T item )
            {
                throw new System.NotImplementedException();
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains( T item )
            {
                return _child.Contains( item );
            }

            public void CopyTo( T[] array, int arrayIndex )
            {
                _child.CopyTo( array, arrayIndex );
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _child.GetEnumerator();
            }

            public int IndexOf( T item )
            {
                return _child.IndexOf( item );
            }

            public void Insert( int index, T item )
            {
                throw new System.NotImplementedException();
            }

            public bool Remove( T item )
            {
                throw new System.NotImplementedException();
            }

            public void RemoveAt( int index )
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _child.GetEnumerator();
            }
        }
    }
}

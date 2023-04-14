using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ArrayTests
    {
        public class User
        {
            public int Id { get; set; }
        }

        public class Test
        {
            public List<User> Users { get; set; }
        }

        //IComparable is required to test sorted collections
        protected class ComplexType : IComparable<ComplexType>
        {
            public int A { get; set; }
            public InnerType InnerType { get; set; }

            public int CompareTo( ComplexType other )
            {
                return this.A.CompareTo( other.A );
            }

            public override int GetHashCode()
            {
                return this.A;
            }

            public override bool Equals( object obj )
            {
                if( !(obj is ComplexType otherObj) ) return false;

                return this.A.Equals( otherObj?.A ) &&
                    (this.InnerType == null && otherObj.InnerType == null) ||
                    ((this.InnerType != null && otherObj.InnerType != null) &&
                        this.InnerType.Equals( otherObj.InnerType ));
            }
        }

        protected class InnerType
        {
            public string String { get; set; }
        }

        protected class GenericCollections<T>
        {
            public T[] Array { get; set; }
            public HashSet<T> HashSet { get; set; }
            public SortedSet<T> SortedSet { get; set; }
            public List<T> List { get; set; }
            public Stack<T> Stack { get; set; }
            public Queue<T> Queue { get; set; }
            public LinkedList<T> LinkedList { get; set; }
            public ObservableCollection<T> ObservableCollection { get; set; }

            public GenericCollections() { }

            public GenericCollections( bool initializeIfPrimitiveGenericArg, uint minVal = 0, uint maxVal = 10 )
            {
                if( minVal > maxVal )
                    throw new ArgumentException( $"{nameof( maxVal )} must be a value greater or equal to {nameof( minVal )}" );

                this.Array = new T[ maxVal - minVal ];
                this.List = new List<T>();
                this.HashSet = new HashSet<T>();
                this.SortedSet = new SortedSet<T>();
                this.Stack = new Stack<T>();
                this.Queue = new Queue<T>();
                this.LinkedList = new LinkedList<T>();
                this.ObservableCollection = new ObservableCollection<T>();

                if( initializeIfPrimitiveGenericArg )
                    Initialize( minVal, maxVal );
            }

            private void Initialize( uint minval, uint maxval )
            {
                var elementType = typeof( T );
                if( elementType.IsBuiltIn( true ) )
                {
                    for( uint i = 0, v = minval; v < maxval; i++, v++ )
                    {
                        T value = (T)Convert.ChangeType( v,
                            elementType.GetUnderlyingTypeIfNullable() );

                        this.Array[ i ] = value;
                        this.List.Add( value );
                        this.HashSet.Add( value );
                        this.SortedSet.Add( value );
                        this.Stack.Push( value );
                        this.Queue.Enqueue( value );
                        this.LinkedList.AddLast( value );
                        this.ObservableCollection.Add( value );
                    }
                }
            }
        }

        private class ReadOnlyGeneric<T>
        {
            public ReadOnlyCollection<T> Array { get; set; }
            public ReadOnlyCollection<T> HashSet { get; set; }
            public ReadOnlyCollection<T> SortedSet { get; set; }
            public ReadOnlyCollection<T> List { get; set; }
            public ReadOnlyCollection<T> Stack { get; set; }
            public ReadOnlyCollection<T> Queue { get; set; }
            public ReadOnlyCollection<T> LinkedList { get; set; }
            public ReadOnlyCollection<T> ObservableCollection { get; set; }
        }

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

            //var target = new GenericCollections<int>( true );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<GenericArray<int>, GenericCollections<int>>()
                    .MapMember( a => a.Array, b => b.List );
            } );

            //using existing instance = reuse references
            //ultraMapper.Map( source, target );

            var target = ultraMapper.Map<GenericCollections<int>>( source );

            Assert.IsTrue( source.Array.SequenceEqual( target.List ) );
        }

        [TestMethod]
        public void SimpleArrayToCollectionConditional()
        {
            var source = new GenericArray<int>()
            {
                Array = Enumerable.Range( 0, 100 ).ToArray()
            };

            //var target = new GenericCollections<int>( true );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<GenericArray<int>, GenericCollections<int>>()
                    .MapConditionalMember( a => true, () => new int[ 0 ], a => a.Array, b => b.List );
            } );

            //using existing instance = reuse references
            //ultraMapper.Map( source, target );

            var target = ultraMapper.Map<GenericCollections<int>>( source );

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
        public void SimpleCollectionToArrayConditional()
        {
            var source = new GenericCollections<int>( false )
            {
                List = Enumerable.Range( 0, 100 ).ToList()
            };

            var target = new GenericArray<int>();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                  .MapConditionalMember( a => true, () => new List<int>(), a => a.List, b => b.Array );
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
                    .MapMember( a => a.Array, b => b.Array, memCfg => memCfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE )
                    .MapMember( a => a.Array, b => b.List );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ComplexArrayToCollectionConditional()
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
                    .MapConditionalMember( a => true, () => new ComplexType[ 0 ], a => a.Array, b => b.Array, memCfg => memCfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE )
                    .MapConditionalMember( a => true, () => new ComplexType[ 0 ], a => a.Array, b => b.List );
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
        public void ComplexCollectionToArrayConditional()
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
                    .MapConditionalMember( a => true, () => new List<ComplexType>(), a => a.List, b => b.Array );
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


        [TestMethod]
        public void ComplexArrayBehindInterfaceConditional()
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
                cfg.MapTypes( source, target )
                 .MapConditionalMember( s => true, () => (ICollection<InnerType>)new InnerType[ 0 ], s => s.Collection, t => t.Collection );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }



        [TestMethod]
        public void ComplexToSimpleElement()
        {
            var source = new GenericArray<InnerType>()
            {
                Array = Enumerable.Range( 0, 100 ).Select( i =>
                    new InnerType() { String = i.ToString() } ).ToArray()
            };

            var target = new GenericCollections<int>( false );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.Array, b => b.Array );

                cfg.MapTypes<InnerType, int>( inner => Int32.Parse( inner.String ) );
            } );

            ultraMapper.Map( source, target );
            Assert.IsTrue( source.Array.Select( inner => inner.String ).SequenceEqual(
                target.Array.Select( item => item.ToString() ) ) );
        }

        [TestMethod]
        public void ComplexToSimpleElementConditional()
        {
            var source = new GenericArray<InnerType>()
            {
                Array = Enumerable.Range( 0, 100 ).Select( i =>
                    new InnerType() { String = i.ToString() } ).ToArray()
            };

            var target = new GenericCollections<int>( false );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapConditionalMember( a => true, () =>new InnerType[0], a => a.Array, b => b.Array);

                cfg.MapTypes<InnerType, int>( inner => Int32.Parse( inner.String ) );
            } );

            ultraMapper.Map( source, target );
            Assert.IsTrue( source.Array.Select( inner => inner.String ).SequenceEqual(
                target.Array.Select( item => item.ToString() ) ) );
        }



        [TestMethod]
        public void SimpleToComplexElement()
        {
            var source = new GenericCollections<int>( false )
            {
                List = Enumerable.Range( 0, 100 ).ToList()
            };

            var target = new GenericArray<InnerType>();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapMember( a => a.List, b => b.Array );

                cfg.MapTypes<int, InnerType>( i => new InnerType() { String = i.ToString() } );
            } );

            ultraMapper.Map( source, target );
            Assert.IsTrue( source.List.Select( inner => inner.ToString() ).SequenceEqual(
                target.Array.Select( item => item.String ) ) );
        }

        [TestMethod]
        public void SimpleToComplexElementConditional()
        {
            var source = new GenericCollections<int>( false )
            {
                List = Enumerable.Range( 0, 100 ).ToList()
            };

            var target = new GenericArray<InnerType>();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes( source, target )
                    .MapConditionalMember( a => true, () => new List<int>(), a => a.List, b => b.Array );

                cfg.MapTypes<int, InnerType>( i => new InnerType() { String = i.ToString() } );
            } );

            ultraMapper.Map( source, target );
            Assert.IsTrue( source.List.Select( inner => inner.ToString() ).SequenceEqual(
                target.Array.Select( item => item.String ) ) );
        }

    }

    [TestClass]
    public class ArrayCapacity
    {
        private class A
        {
            public int[] Array { get; set; }
        }

        [TestMethod]
        public void ReuseResetInstance()
        {
            var source = new A
            {
                Array = Enumerable.Range( 0, 3 ).ToArray()
            };

            var target = new A() { Array = new int[ 5 ] { 10, 20, 30, 40, 50 } };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                cfg.CollectionBehavior = CollectionBehaviors.RESET;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Array.SequenceEqual( new[] { 0, 1, 2, 0, 0 } ) );
        }

        [TestMethod]
        public void ReuseMergeInstance()
        {
            var source = new A
            {
                Array = Enumerable.Range( 0, 3 ).ToArray()
            };

            var target = new A() { Array = new int[ 5 ] { 10, 20, 30, 40, 50 } };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                cfg.CollectionBehavior = CollectionBehaviors.MERGE;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Array.SequenceEqual( new[] { 0, 1, 2, 40, 50 } ) );
        }

        [TestMethod]
        public void ResizeInstance()
        {
            var source = new A
            {
                Array = Enumerable.Range( 0, 5 ).ToArray()
            };

            var target = new A() { Array = new int[ 3 ] };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                cfg.CollectionBehavior = CollectionBehaviors.MERGE;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Array.SequenceEqual( new[] { 0, 1, 2, 3, 4 } ) );
        }

        [TestMethod]
        public void CreateNewInstanceMerge()
        {
            var source = new A
            {
                Array = Enumerable.Range( 0, 5 ).ToArray()
            };

            var target = new A() { Array = new int[ 3 ] };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
                cfg.CollectionBehavior = CollectionBehaviors.MERGE;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Array.SequenceEqual( new[] { 0, 1, 2, 3, 4 } ) );
        }

        [TestMethod]
        public void CreateNewInstanceReset()
        {
            var source = new A
            {
                Array = Enumerable.Range( 0, 5 ).ToArray()
            };

            var target = new A() { Array = new int[ 3 ] };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
                cfg.CollectionBehavior = CollectionBehaviors.RESET;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Array.SequenceEqual( new[] { 0, 1, 2, 3, 4 } ) );
        }
    }
}

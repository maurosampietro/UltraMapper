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
    public class CollectionTests
    {
        private static readonly Random _random = new Random();

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
                var otherObj = obj as ComplexType;
                if( otherObj == null ) return false;

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
                if( elementType.IsBuiltInType( true ) )
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

        [TestMethod]
        public void CollectionItemsAndMembersMapping()
        {
            var source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 100;
            var target = new List<double>() { 1, 2, 3 };

            Assert.IsTrue( !source.SequenceEqual(
                    target.Select( item => (int)item ) ) );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual(
                target.Select( item => (int)item ) ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void MergeCollections()
        {
            var source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 100;
            var target = new List<double>() { 1, 2, 3 };

            Assert.IsTrue( !source.SequenceEqual(
                    target.Select( item => (int)item ) ) );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior =
                    ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual(
                target.Select( item => (int)item ) ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void PrimitiveCollection()
        {
            var excludeTypes = new TypeCode[]
            {
                TypeCode.Empty,
                TypeCode.DBNull,
                TypeCode.DateTime, //DateTime is not managed
                TypeCode.Object,
                TypeCode.Boolean, //Bool flattens value to 0 or 1 so hashset differ too much. change the verifier to take care of conversions
            };

            var types = Enum.GetValues( typeof( TypeCode ) ).Cast<TypeCode>()
                .Except( excludeTypes )
                .Select( typeCode => TypeExtensions.GetType( typeCode ) ).ToList();

            foreach( var sourceElementType in types )
            {
                foreach( var targetElementType in types )
                {
                    //for the following pairs a conversion is known
                    //to be harder (not possible or convention-based), 
                    //so here we just skip that few cases

                    if( sourceElementType == typeof( string ) &&
                        targetElementType == typeof( bool ) ) continue;

                    if( sourceElementType == typeof( char ) &&
                        targetElementType == typeof( bool ) ) continue;

                    if( sourceElementType == typeof( bool ) &&
                        targetElementType == typeof( char ) ) continue;


                    var sourceType = typeof( GenericCollections<> )
                        .MakeGenericType( sourceElementType );

                    var targetType = typeof( GenericCollections<> )
                        .MakeGenericType( targetElementType );

                    var sourceTypeCtor = ConstructorFactory.CreateConstructor<bool, uint, uint>( sourceType );
                    var targetTypeCtor = ConstructorFactory.CreateConstructor<bool, uint, uint>( targetType );

                    var source = sourceTypeCtor( true, 0, 10 );
                    var target = targetTypeCtor( false, 0, 10 );

                    var ultraMapper = new Mapper();
                    ultraMapper.Map( source, target );

                    bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                    Assert.IsTrue( isResultOk );
                }
            }
        }

        [TestMethod]
        public void NullablePrimitiveCollection()
        {
            //DateTime is not managed
            var nullableTypes = new Type[]
            {
               // typeof( bool? ),//Bool flattens value to 0 or 1 so hashset differ too much. change the verifier to take care of conversions
                typeof( char? ),
                typeof( sbyte? ),
                typeof( byte? ),
                typeof( int? ),
                typeof( uint? ),
                typeof( int? ),
                typeof( uint? ),
                typeof( int? ),
                typeof( uint? ),
                typeof( float? ),
                typeof( double? ),
                typeof( decimal? ),
                //typeof( string )
            };

            foreach( var sourceElementType in nullableTypes )
            {
                foreach( var targetElementType in nullableTypes )
                {
                    if( sourceElementType == typeof( char? ) &&
                        targetElementType == typeof( bool? ) ) continue;

                    if( sourceElementType == typeof( bool? ) &&
                        targetElementType == typeof( char? ) ) continue;

                    //for the following pairs a conversion is known
                    //to be harder (not possible or convention-based), 
                    //so here we just skip that few cases
                    if( sourceElementType == typeof( bool? ) &&
                        targetElementType == typeof( string ) ) continue;

                    if( sourceElementType == typeof( string ) &&
                        targetElementType == typeof( bool? ) ) continue;

                    var sourceType = typeof( GenericCollections<> )
                        .MakeGenericType( sourceElementType );

                    var targetType = typeof( GenericCollections<> )
                        .MakeGenericType( targetElementType );

                    var sourceTypeCtor = ConstructorFactory.CreateConstructor<bool, uint, uint>( sourceType );
                    var targetTypeCtor = ConstructorFactory.CreateConstructor<bool, uint, uint>( targetType );

                    var source = sourceTypeCtor( true, 0, 10 );
                    var target = targetTypeCtor( true, 0, 10 );

                    var ultraMapper = new Mapper();
                    ultraMapper.Map( source, target );

                    bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                    Assert.IsTrue( isResultOk );
                }
            }
        }

        [TestMethod]
        public void DirectArrayToCollection()
        {
            var source = Enumerable.Range( 0, 100 ).ToArray();
            var target = new List<int>();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual( target ) );
        }

        [TestMethod]
        public void CollectionToReadOnlySimpleCollection()
        {
            var source = new GenericCollections<int>( true );
            var target = new ReadOnlyGeneric<int>();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void CollectionToReadOnlyComplexCollection()
        {
            var innerType = new InnerType() { String = "test" };

            var source = new GenericCollections<ComplexType>( false );
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i, InnerType = innerType };
                source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
                source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
                source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
                source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
            }

            var target = new ReadOnlyGeneric<ComplexType>();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ComplexCollection()
        {
            var innerType = new InnerType() { String = "test" };

            var source = new GenericCollections<ComplexType>( false );
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i, InnerType = innerType };
                source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
                source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
                source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
                source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
            }

            var target = new GenericCollections<ComplexType>( false );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );

            Assert.IsTrue( !Object.ReferenceEquals( source.HashSet.First().InnerType, target.HashSet.First().InnerType ) );

            Assert.IsTrue( target.List.Concat( target.HashSet.Concat( target.SortedSet.Concat( target.Stack.Concat(
                target.Queue.Concat( target.LinkedList.Concat( target.ObservableCollection ) ) ) ) ) )
                .Select( it => it.InnerType )
                .All( item => Object.ReferenceEquals( item, target.HashSet.First().InnerType ) ) );
        }

        [TestMethod]
        public void FromPrimitiveCollectionToAnother()
        {
            var sourceProperties = typeof( GenericCollections<int> ).GetProperties();
            var targetProperties = typeof( GenericCollections<double> ).GetProperties();

            var source = new GenericCollections<int>( false );

            //initialize source
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = i;
                source.List.Add( i );
                source.HashSet.Add( i );
                source.SortedSet.Add( i );
                source.Stack.Push( i );
                source.Queue.Enqueue( i );
                source.LinkedList.AddLast( i );
                source.ObservableCollection.Add( i );
            }

            foreach( var sourceProp in sourceProperties )
            {
                var target = new GenericCollections<double>( false );

                var ultraMapper = new Mapper();
                var typeMappingConfig = ultraMapper.MappingConfiguration.MapTypes( source, target );

                foreach( var targetProp in targetProperties )
                    typeMappingConfig.MapMember( sourceProp, targetProp );

                ultraMapper.Map( source, target );

                bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                Assert.IsTrue( isResultOk );
            }
        }

        [TestMethod]
        public void FromComplexCollectionToAnother()
        {
            var typeProperties = typeof( GenericCollections<ComplexType> ).GetProperties();

            var source = new GenericCollections<ComplexType>( false );

            //initialize source
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i };
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            foreach( var sourceProp in typeProperties )
            {
                var ultraMapper = new Mapper( cfg =>
                {
                    //cfg.GlobalConfiguration.IgnoreConventions = true;
                } );

                var target = new GenericCollections<ComplexType>( false );

                var typeMappingConfig = ultraMapper.MappingConfiguration.MapTypes( source, target );
                foreach( var targetProp in typeProperties )
                    typeMappingConfig.MapMember( sourceProp, targetProp );

                ultraMapper.Map( source, target );

                bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
                Assert.IsTrue( isResultOk );
            }
        }

        [TestMethod]
        public void AssignNullCollection()
        {
            var source = new GenericCollections<int>( false )
            {
                Array = null,
                List = null,
                HashSet = null,
                SortedSet = null,
                Stack = null,
                Queue = null,
                LinkedList = null,
                ObservableCollection = null
            };

            var target = new GenericCollections<int>( true );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void AssignToNullCollection()
        {
            var source = new GenericCollections<ComplexType>( false );
            //initialize source
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i };
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollections<ComplexType>( true )
            {
                Array = null,
                List = null,
                HashSet = null,
                SortedSet = null,
                Stack = null,
                Queue = null,
                LinkedList = null,
                ObservableCollection = null
            };

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UpdateAssignToNullCollection()
        {
            var source = new GenericCollections<ComplexType>( false );
            //initialize source
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i };
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollections<ComplexType>( true )
            {
                Array = null,
                List = null,
                HashSet = null,
                SortedSet = null,
                Stack = null,
                Queue = null,
                LinkedList = null,
                ObservableCollection = null
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<IEnumerable<ComplexType>, IEnumerable<ComplexType>, ComplexType, ComplexType>(
                    ( itemA, itemB ) => Comparison( itemA, itemB ) );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void KeepAndClearCollection()
        {
            var source = new GenericCollections<ComplexType>( false );

            //initialize source
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i };
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollections<ComplexType>( false )
            {
                List = new List<ComplexType>() { new ComplexType() { A = 100 } }
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.IgnoreMemberMappingResolvedByConvention = true;

                cfg.MapTypes<ComplexType, ComplexType>( typeCfg =>
                {
                    typeCfg.IgnoreMemberMappingResolvedByConvention = false;
                } );

                cfg.MapTypes<GenericCollections<ComplexType>, GenericCollections<ComplexType>>()
                   .MapMember( a => a.List, b => b.List, memberConfig =>
                   {
                       memberConfig.CollectionBehavior = CollectionBehaviors.RESET;
                       memberConfig.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                   } );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        bool Comparison( ComplexType itemA, ComplexType itemB ) { return itemA?.A == itemB?.A; }

        [TestMethod]
        public void CollectionUpdate()
        {
            var innerType = new InnerType() { String = "test" };
            var source = new GenericCollections<ComplexType>( false );

            //initialize source
            for( int i = 0; i < 10; i++ )
            {
                source.Array[ i ] = new ComplexType() { A = i, InnerType = innerType };
                source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
                source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
                source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
                source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
                source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
            }

            var tempItemA = new ComplexType() { A = 1 };
            var tempItemB = new ComplexType() { A = 9 };

            var target = new GenericCollections<ComplexType>( false )
            {
                Array = new ComplexType[ 10 ] { tempItemA, tempItemB, new ComplexType() { A = 10 }, null, null, null, null, null, null, null },
                List = new List<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } },
                HashSet = new HashSet<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } },
                SortedSet = new SortedSet<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } },
                ObservableCollection = new ObservableCollection<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } }
            };

            target.Stack = new Stack<ComplexType>();
            target.Stack.Push( tempItemA );
            target.Stack.Push( tempItemB );
            target.Stack.Push( new ComplexType() { A = 10 } );

            target.Queue = new Queue<ComplexType>();
            target.Queue.Enqueue( tempItemA );
            target.Queue.Enqueue( tempItemB );
            target.Queue.Enqueue( new ComplexType() { A = 10 } );

            target.LinkedList = new LinkedList<ComplexType>();
            target.LinkedList.AddLast( tempItemA );
            target.LinkedList.AddLast( tempItemB );
            target.LinkedList.AddLast( new ComplexType() { A = 10 } );

            //item comparer should also check for nulls ALWAYS
            Expression<Func<ComplexType, ComplexType, bool>> itemComparer =
                ( itemA, itemB ) => Comparison( itemA, itemB );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<IEnumerable<ComplexType>, IEnumerable<ComplexType>>( cfg2 =>
                {
                    cfg2.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                    cfg2.CollectionBehavior = CollectionBehaviors.UPDATE;
                    cfg2.CollectionItemEqualityComparer = itemComparer;
                } );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.Array.Length == source.Array.Length );
            Assert.IsTrue( object.ReferenceEquals( target.Array.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.Array.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.Array )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.List.Count == source.List.Count );
            Assert.IsTrue( object.ReferenceEquals( target.List.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.List.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.List )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.HashSet.Count == source.HashSet.Count );
            Assert.IsTrue( object.ReferenceEquals( target.HashSet.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.HashSet.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.HashSet )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.SortedSet.Count == source.SortedSet.Count );
            Assert.IsTrue( object.ReferenceEquals( target.SortedSet.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.SortedSet.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.SortedSet )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.ObservableCollection.Count == source.ObservableCollection.Count );
            Assert.IsTrue( object.ReferenceEquals( target.ObservableCollection.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.ObservableCollection.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.ObservableCollection )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.LinkedList.Count == source.LinkedList.Count );
            Assert.IsTrue( object.ReferenceEquals( target.LinkedList.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.LinkedList.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.LinkedList )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.Stack.Count == source.Stack.Count );
            Assert.IsTrue( object.ReferenceEquals( target.Stack.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.Stack.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.Stack )
                Assert.IsTrue( item.InnerType != null );


            Assert.IsTrue( target.Queue.Count == source.Queue.Count );
            Assert.IsTrue( object.ReferenceEquals( target.Queue.First( item => item.A == 1 ), tempItemA ) );
            Assert.IsTrue( object.ReferenceEquals( target.Queue.First( item => item.A == 9 ), tempItemB ) );

            foreach( var item in target.Queue )
                Assert.IsTrue( item.InnerType != null );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void NullItemsInCollection()
        {
            var _mapper = new UltraMapper.Mapper();

            var users = new Test()
            {
                Users = new List<User>()
            };

            var newUsers = new Test()
            {
                Users = new List<User>() { null, null, null }
            };

            _mapper.Map( newUsers, users );
        }

        //[TestMethod]
        //public void MergeCollectionKeepingTargetInstance()
        //{
        //    throw new NotImplementedException();
        //}

        //[TestMethod]
        //public void MergeCollectionCreatingNewTargetInstance()
        //{
        //    var innerType = new InnerType() { String = "test" };
        //    var source = new GenericCollections<ComplexType>( false );

        //    //initialize source
        //    for( int i = 0; i < 10; i++ )
        //    {
        //        source.Array[ i ] = new ComplexType() { A = i, InnerType = innerType };
        //        source.List.Add( new ComplexType() { A = i, InnerType = innerType } );
        //        source.HashSet.Add( new ComplexType() { A = i, InnerType = innerType } );
        //        source.SortedSet.Add( new ComplexType() { A = i, InnerType = innerType } );
        //        source.Stack.Push( new ComplexType() { A = i, InnerType = innerType } );
        //        source.Queue.Enqueue( new ComplexType() { A = i, InnerType = innerType } );
        //        source.LinkedList.AddLast( new ComplexType() { A = i, InnerType = innerType } );
        //        source.ObservableCollection.Add( new ComplexType() { A = i, InnerType = innerType } );
        //    }

        //    var tempItemA = new ComplexType() { A = 1 };
        //    var tempItemB = new ComplexType() { A = 9 };

        //    var target = new GenericCollections<ComplexType>( false )
        //    {
        //        Array = new ComplexType[ 10 ] { tempItemA, tempItemB, new ComplexType() { A = 10 }, null, null, null, null, null, null, null },
        //        List = new List<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } },
        //        HashSet = new HashSet<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } },
        //        SortedSet = new SortedSet<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } },
        //        ObservableCollection = new ObservableCollection<ComplexType>() { tempItemA, tempItemB, new ComplexType() { A = 10 } }
        //    };

        //    var tempStack = target.Stack = new Stack<ComplexType>();
        //    target.Stack.Push( tempItemA );
        //    target.Stack.Push( tempItemB );
        //    target.Stack.Push( new ComplexType() { A = 10 } );

        //    var tempQueue = target.Queue = new Queue<ComplexType>();
        //    target.Queue.Enqueue( tempItemA );
        //    target.Queue.Enqueue( tempItemB );
        //    target.Queue.Enqueue( new ComplexType() { A = 10 } );

        //    var tempLinkedList = target.LinkedList = new LinkedList<ComplexType>();
        //    target.LinkedList.AddLast( tempItemA );
        //    target.LinkedList.AddLast( tempItemB );
        //    target.LinkedList.AddLast( new ComplexType() { A = 10 } );

        //    //item comparer should also check for nulls ALWAYS
        //    Expression<Func<ComplexType, ComplexType, bool>> itemComparer =
        //        ( itemA, itemB ) => Comparison( itemA, itemB );

        //    var ultraMapper = new Mapper( cfg =>
        //    {
        //        cfg.MapTypes<IEnumerable<ComplexType>, IEnumerable<ComplexType>>( cfg2 =>
        //        {
        //            cfg2.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
        //            cfg2.CollectionBehavior = CollectionBehaviors.MERGE;
        //            cfg2.CollectionItemEqualityComparer = itemComparer;
        //        } );
        //    } );

        //    //!!! questo overload di default imposta internamente ReferenceBehaviors = USE_TARGET_INSTANCE_IF_NOT_NULL
        //    // per gestire alcuni casi limite
        //    ultraMapper.Map( source, target );

        //    Assert.IsTrue( !Object.ReferenceEquals( target.LinkedList, tempLinkedList ) );
        //    Assert.IsTrue( !Object.ReferenceEquals( target.Stack, tempStack ) );
        //    Assert.IsTrue( !Object.ReferenceEquals( target.Queue, tempQueue ) );
        //}

        [TestMethod]
        public void SimpleCollectionUpdate()
        {
            var source = new GenericCollections<int>( true, 0, 10 );
            var target = new GenericCollections<int>( true, 5, 15 );
            var check = new GenericCollections<int>( true, 5, 15 );

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.CollectionBehavior = CollectionBehaviors.MERGE;

                cfg.MapTypes<IEnumerable<int>, IEnumerable<int>>( ( ITypeOptions op ) =>
                    op.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( target.List.SequenceEqual(
                check.List.Concat( source.List ) ) );

            Assert.IsTrue( target.LinkedList.SequenceEqual(
                check.LinkedList.Concat( source.LinkedList ) ) );

            Assert.IsTrue( target.ObservableCollection.SequenceEqual(
                check.ObservableCollection.Concat( source.ObservableCollection ) ) );

            Assert.IsTrue( check.SortedSet.Concat( source.SortedSet )
                .All( item => target.SortedSet.Contains( item ) ) );

            Assert.IsTrue( check.HashSet.Concat( source.HashSet )
                .All( item => target.HashSet.Contains( item ) ) );

            Assert.IsTrue( target.Queue.SequenceEqual(
              check.Queue.Concat( source.Queue ) ) );

            Assert.IsTrue( target.Stack.SequenceEqual(
                source.Stack.Concat( check.Stack ) ) );
        }
    }
}

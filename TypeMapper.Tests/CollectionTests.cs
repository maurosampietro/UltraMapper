using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.MappingConventions;

namespace TypeMapper.Tests
{
    [TestClass]
    public class CollectionTests
    {
        private static Random _random = new Random();

        //IComparable is required to test sorted collections
        private class ComplexType : IComparable<ComplexType>
        {
            public int A { get; set; }

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
                return this.A.Equals( (obj as ComplexType)?.A );
            }
        }

        private class GenericCollections<T>
        {
            public List<T> List { get; set; }
            public HashSet<T> HashSet { get; set; }
            public SortedSet<T> SortedSet { get; set; }
            public Stack<T> Stack { get; set; }
            public Queue<T> Queue { get; set; }
            public LinkedList<T> LinkedList { get; set; }
            public ObservableCollection<T> ObservableCollection { get; set; }

            public GenericCollections( bool initializeRandomly )
                : this()
            {
                InitializeRandomly();
            }

            public GenericCollections()
            {
                this.List = new List<T>();
                this.HashSet = new HashSet<T>();
                this.SortedSet = new SortedSet<T>();
                this.Stack = new Stack<T>();
                this.Queue = new Queue<T>();
                this.LinkedList = new LinkedList<T>();
                this.ObservableCollection = new ObservableCollection<T>();
            }

            private void InitializeRandomly()
            {
                var elementType = typeof( T );

                for( int i = 0; i < 10; i++ )
                {
                    T value = (T)Convert.ChangeType( i, 
                        elementType.GetUnderlyingTypeIfNullable() );

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

        [TestMethod]
        public void PrimitiveCollection()
        {
            var excludeTypes = new TypeCode[]
            {
                TypeCode.Empty,
                TypeCode.DBNull,
                TypeCode.DateTime, //DateTime is not managed
                TypeCode.Object,
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

                    var sourceTypeCtor = ConstructorFactory.GetOrCreateConstructor<bool>( sourceType );
                    var targetTypeCtor = ConstructorFactory.GetOrCreateConstructor<bool>( targetType );

                    var source = sourceTypeCtor( true );
                    var target = targetTypeCtor( true );

                    var typeMapper = new TypeMapper();
                    typeMapper.Map( source, target );

                    bool isResultOk = typeMapper.VerifyMapperResult( source, target );
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
                typeof( bool? ),
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
                    if( sourceElementType == typeof( bool? ) &&
                        targetElementType == typeof( string ) ) continue;

                    //if( sourceElementType == typeof( string ) &&
                    //    targetElementType == typeof( bool?) ) continue;

                    if( sourceElementType == typeof( char? ) &&
                        targetElementType == typeof( bool? ) ) continue;

                    if( sourceElementType == typeof( bool? ) &&
                        targetElementType == typeof( char? ) ) continue;

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

                    var sourceTypeCtor = ConstructorFactory.GetOrCreateConstructor<bool>( sourceType );
                    var targetTypeCtor = ConstructorFactory.GetOrCreateConstructor<bool>( targetType );

                    var source = sourceTypeCtor( true );
                    var target = targetTypeCtor( true );

                    var typeMapper = new TypeMapper();
                    typeMapper.Map( source, target );

                    bool isResultOk = typeMapper.VerifyMapperResult( source, target );
                    Assert.IsTrue( isResultOk );
                }
            }
        }

        [TestMethod]
        public void ComplexCollection()
        {
            var source = new GenericCollections<ComplexType>();
            for( int i = 0; i < 50; i++ )
            {
                source.List.Add( new ComplexType() { A = i } );
                source.HashSet.Add( new ComplexType() { A = i } );
                source.SortedSet.Add( new ComplexType() { A = i } );
                source.Stack.Push( new ComplexType() { A = i } );
                source.Queue.Enqueue( new ComplexType() { A = i } );
                source.LinkedList.AddLast( new ComplexType() { A = i } );
                source.ObservableCollection.Add( new ComplexType() { A = i } );
            }

            var target = new GenericCollections<ComplexType>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void FromPrimitiveCollectionToAnother()
        {
            var sourceProperties = typeof( GenericCollections<int> ).GetProperties();
            var targetProperties = typeof( GenericCollections<double> ).GetProperties();

            foreach( var sourceProp in sourceProperties )
            {
                var source = new GenericCollections<int>();

                //initialize source
                for( int i = 0; i < 50; i++ )
                {
                    source.List.Add( i );
                    source.HashSet.Add( i );
                    source.SortedSet.Add( i );
                    source.Stack.Push( i );
                    source.Queue.Enqueue( i );
                    source.LinkedList.AddLast( i );
                    source.ObservableCollection.Add( i );
                }

                var target = new GenericCollections<double>();

                var typeMapper = new TypeMapper();
                var typeMappingConfig = typeMapper.MappingConfiguration.MapTypes( source, target );

                foreach( var targetProp in targetProperties )
                    typeMappingConfig.MapProperty( sourceProp, targetProp );

                typeMapper.Map( source, target );

                bool isResultOk = typeMapper.VerifyMapperResult( source, target );
                Assert.IsTrue( isResultOk );
            }
        }

        [TestMethod]
        public void FromComplexCollectionToAnother()
        {
            var typeProperties = typeof( GenericCollections<ComplexType> ).GetProperties();

            foreach( var sourceProp in typeProperties )
            {
                var source = new GenericCollections<ComplexType>();

                //initialize source
                for( int i = 0; i < 50; i++ )
                {
                    source.List.Add( new ComplexType() { A = i } );
                    source.HashSet.Add( new ComplexType() { A = i } );
                    source.SortedSet.Add( new ComplexType() { A = i } );
                    source.Stack.Push( new ComplexType() { A = i } );
                    source.Queue.Enqueue( new ComplexType() { A = i } );
                    source.LinkedList.AddLast( new ComplexType() { A = i } );
                    source.ObservableCollection.Add( new ComplexType() { A = i } );
                }

                var typeMapper = new TypeMapper( cfg =>
                {
            //cfg.GlobalConfiguration.IgnoreConventions = true;
        } );

                var target = new GenericCollections<ComplexType>();

                var typeMappingConfig = typeMapper.MappingConfiguration.MapTypes( source, target );
                foreach( var targetProp in typeProperties )
                    typeMappingConfig.MapProperty( sourceProp, targetProp );

                typeMapper.Map( source, target );

                bool isResultOk = typeMapper.VerifyMapperResult( source, target );
                Assert.IsTrue( isResultOk );
            }
        }

        [TestMethod]
        public void AssignNullCollection()
        {
            var source = new GenericCollections<int>()
            {
                List = null,
                HashSet = null,
                SortedSet = null,
                Stack = null,
                Queue = null,
                LinkedList = null,
                ObservableCollection = null
            };

            var target = new GenericCollections<int>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

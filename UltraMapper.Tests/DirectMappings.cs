using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class DirectMappings
    {
        private static readonly ComplexTypeComparer comparer = new ComplexTypeComparer();
        private class ComplexTypeComparer : IEqualityComparer<ComplexType>
        {
            public bool Equals( ComplexType x, ComplexType y )
            {
                return x.PropertyA == y.PropertyA &&
                    (x.InnerType == null && y.InnerType == null ||
                    x.InnerType.String == y.InnerType.String);
            }

            public int GetHashCode( ComplexType obj )
            {
                int hash = obj.PropertyA;
                if( obj.InnerType != null )
                    hash ^= obj.InnerType.String.GetHashCode();

                return hash;
            }
        }

        private class ComplexType
        {
            public int PropertyA { get; set; }
            public InnerType InnerType { get; set; }
        }

        private class InnerType
        {
            public string String { get; set; }
        }

        [TestMethod]
        public void PrimitiveToSamePrimitive()
        {
            int source = 10;
            int target = 13;

            Assert.IsTrue( source != target );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, out target );

            Assert.IsTrue( source == target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void PrimitiveToDifferentPrimitive()
        {
            int source = 10;
            double target = 13;

            Assert.IsTrue( source != target );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, out target );

            Assert.IsTrue( source == target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ToNullablePrimitiveArray()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( i => new int?( i ) )
                .Concat( new int?[] { null } )
                .ToList();

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<int?[]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ToPrimitiveArray()
        {
            var source = Enumerable.Range( 0, 10 ).ToList();

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<int[]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ToPrimitiveMultidimensionalArray()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( i => Enumerable.Range( 1, 2 ).ToList() )
                .ToList();

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<int[][]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsFalse( Object.ReferenceEquals( source[ 0 ], target[ 0 ] ) );
        }

        [TestMethod]
        public void ToNonPrimitiveArray()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( i => new ComplexType() { PropertyA = i } )
                .ToList();

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<ComplexType[]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ToComplexMultidimensionalArray()
        {
            var source = new List<List<ComplexType>>()
            {
               new List<ComplexType>(){  new ComplexType() { PropertyA = 1 }, new ComplexType() { PropertyA = 2 } },
               new List<ComplexType>(){  new ComplexType() { PropertyA = 1 }, new ComplexType() { PropertyA = 2 } }
            };

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<ComplexType[][]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void DictionaryToDictionarySameElementSimpleType()
        {
            var source = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 3 } };
            var target = new Dictionary<int, int>();

            Assert.IsTrue( !source.SequenceEqual( target ) );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual( target ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ListToListSameElementSimpleType()
        {
            var source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 1000;
            var target = Enumerable.Range( 10, 10 ).ToList();

            Assert.IsTrue( !source.SequenceEqual( target ) );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual( target ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ListToListSameElementComplexType()
        {
            var innerType = new InnerType() { String = "test" };
            var source = new List<ComplexType>()
            {
                new ComplexType() { PropertyA = 1, InnerType = innerType },
                new ComplexType() { PropertyA = 2, InnerType = innerType }
            };

            var target = new List<ComplexType>();

            Assert.IsTrue( !source.SequenceEqual( target ) );

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual( target, comparer ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ListToListDifferentElementType()
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
        public void FromPrimitiveCollectionToComplexCollection()
        {
            var source = new List<int>() { 11, 13, 17 };

            var target = new List<ComplexType>()
            {
                new ComplexType() { PropertyA = 1 },
                new ComplexType() { PropertyA = 2 }
            };

            var ultraMapper = new Mapper
            (
                cfg => cfg.MapTypes<int, ComplexType>( c => new ComplexType() { PropertyA = c } )
            );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void FromComplexCollectionToPrimitiveCollection()
        {
            var source = new List<ComplexType>()
            {
                new ComplexType() { PropertyA = 1 },
                new ComplexType() { PropertyA = 2 }
            };

            var target = new List<int>() { 11, 13, 17 };

            var ultraMapper = new Mapper
            (
                cfg => cfg.MapTypes<ComplexType, int>( a => a.PropertyA )
            );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        [Ignore]
        public void UnmaterializedMultidimensionalArray()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( i => Enumerable.Range( 1, 2 ) /*no .ToList(), no .ToArray() etc..*/ );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<int[][]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedEnumerableToPrimitiveArray()
        {
            var source = Enumerable.Range( 0, 10 );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<int[]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedEnumerableToComplexArray()
        {
            //There's no .ToList() or .ToArray() or anything else here.
            var source = Enumerable.Range( 0, 10 )
                .Select( i => new ComplexType() { PropertyA = i } );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<ComplexType[]>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedToComplexMultidimensionalArray()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( item => Enumerable.Range( 0, 2 ).Select(
                    subItem => new ComplexType() { PropertyA = item } ) );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<List<List<ComplexType>>>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedMultidimensionalList()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( i => Enumerable.Range( 1, 2 ) /*no .ToList(), no .ToArray() etc..*/ );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<List<List<int>>>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedEnumerableToPrimitiveList()
        {
            var source = Enumerable.Range( 0, 10 );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<List<int>>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedEnumerableToComplexList()
        {
            //There's no .ToList() or .ToList() or anything else here.
            var source = Enumerable.Range( 0, 10 )
                .Select( i => new ComplexType() { PropertyA = i } );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<List<ComplexType>>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UnmaterializedToComplexMultidimensionalList()
        {
            var source = Enumerable.Range( 0, 10 )
                .Select( item => Enumerable.Range( 0, 2 ).Select(
                    subItem => new ComplexType() { PropertyA = item } ) );

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<List<List<ComplexType>>>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

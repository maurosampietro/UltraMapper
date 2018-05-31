using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class DirectMappings
    {
        private static ComplexTypeComparer comparer = new ComplexTypeComparer();
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
        public void DictionaryToDictionarySameElementSimpleType()
        {
            Dictionary<int, int> source = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 3 } };
            Dictionary<int, int> target = new Dictionary<int, int>();

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
            List<int> source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 1000;
            List<int> target = Enumerable.Range( 10, 10 ).ToList();

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
            List<int> source = Enumerable.Range( 0, 10 ).ToList();
            source.Capacity = 100;
            List<double> target = new List<double>() { 1, 2, 3 };

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
    }
}

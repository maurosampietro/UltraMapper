using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Tests
{
    [TestClass]
    public class DictionaryTests
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

        private class ComplexType2 : IComparable<ComplexType>
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

        private class GenericDictionaries<TKey, TValue>
        {
            public Dictionary<TKey, TValue> Dictionary { get; set; }

            public GenericDictionaries()
            {
                this.Dictionary = new Dictionary<TKey, TValue>();
            }
        }

        [TestMethod]
        public void SimpleDictionaryTest()
        {
            var source = new GenericDictionaries<int, int>()
            {
                Dictionary = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 3 } }
            };

            var target = new GenericDictionaries<int, int>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void PrimitiveToOtherPrimitiveDictionaryTest()
        {
            var source = new GenericDictionaries<int, double>()
            {
                Dictionary = new Dictionary<int, double>() { { 1, 1 }, { 2, 2 }, { 3, 3 } }
            };

            var target = new GenericDictionaries<double, int>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ComplexToComplexDictionaryTest()
        {
            var source = new GenericDictionaries<int, ComplexType>()
            {
                Dictionary = new Dictionary<int, ComplexType>() { { 1, new ComplexType() { A = 29 } } }
            };

            var target = new GenericDictionaries<double, ComplexType>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ComplexToAnotherComplexDictionaryTest()
        {
            var source = new GenericDictionaries<int, ComplexType>()
            {
                Dictionary = new Dictionary<int, ComplexType>() { { 1, new ComplexType() { A = 29 } } }
            };

            var target = new GenericDictionaries<double, ComplexType2>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( isResultOk );
        }
    }
}

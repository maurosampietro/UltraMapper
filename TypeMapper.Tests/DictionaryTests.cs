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
            var source = new GenericDictionaries<int, int>()
            {
                Dictionary = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 }, { 3, 3 } }
            };

            var target = new GenericDictionaries<double, double>();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

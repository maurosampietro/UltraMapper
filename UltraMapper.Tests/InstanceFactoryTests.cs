using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InstanceFactoryStrongTypedTests
    {
        private class Complex { }

        #region Arrays
        [TestMethod]
        public void CreatePrimitiveArrayStrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int[]>( 2 );
            Assert.IsTrue( instance.Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank2StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int[][]>( 2, 2 );

            Assert.IsTrue( instance[ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank3StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int, int[][][]>( 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank4StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int, int, int[][][][]>( 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank5StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int, int, int, int[][][][][]>( 2, 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayStrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, Complex[]>( 2 );
            Assert.IsTrue( instance.Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank2StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, Complex[][]>( 2, 2 );

            Assert.IsTrue( instance[ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank3StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int, Complex[][][]>( 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank4StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int, int, Complex[][][][]>( 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank5StrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int, int, int, int, int, Complex[][][][][]>( 2, 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 1 ].Length == 2 );
        }
        #endregion
    }

    [TestClass]
    public class InstanceFactoryWeakTypedTests
    {
        private class Complex { }

        [TestMethod]
        public void CreatePrimitiveArray()
        {
            var instance = (int[])InstanceFactory.CreateObject( typeof( int[] ), 2 );
            Assert.IsTrue( instance.Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank2()
        {
            var instance = (int[][])InstanceFactory.CreateObject( typeof( int[][] ), 2, 2 );
            Assert.IsTrue( instance[ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank3()
        {
            var instance = (int[][][])InstanceFactory.CreateObject( typeof( int[][][] ), 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank4StrongTyped()
        {
            var instance = (int[][][][])InstanceFactory.CreateObject( typeof( int[][][][] ), 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreatePrimitiveArrayRank5StrongTyped()
        {
            var instance = (int[][][][][])InstanceFactory.CreateObject( typeof( int[][][][][] ), 2, 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArray()
        {
            var instance = (Complex[])InstanceFactory.CreateObject( typeof( Complex[] ), 2 );
            Assert.IsTrue( instance.Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank2()
        {
            var instance = (Complex[][])InstanceFactory.CreateObject( typeof( Complex[][] ), 2, 2 );
            Assert.IsTrue( instance[ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank3()
        {
            var instance = (Complex[][][])InstanceFactory.CreateObject( typeof( Complex[][][] ), 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank4StrongTyped()
        {
            var instance = (Complex[][][][])InstanceFactory.CreateObject( typeof( Complex[][][][] ), 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 1 ][ 1 ][ 1 ].Length == 2 );
        }

        [TestMethod]
        public void CreateComplexArrayRank5StrongTyped()
        {
            var instance = (Complex[][][][][])InstanceFactory.CreateObject( typeof( Complex[][][][][] ), 2, 2, 2, 2, 2 );

            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 0 ][ 1 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 0 ][ 1 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 0 ].Length == 2 );
            Assert.IsTrue( instance[ 0 ][ 1 ][ 1 ][ 1 ].Length == 2 );
        }
    }
}

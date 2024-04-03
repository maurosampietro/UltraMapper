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
        private class Complex
        {
            public int Arg1 { get; }
            public string Arg2 { get; }

            public Complex() { }

            public Complex( int arg1 )
            {
                this.Arg1 = arg1;
            }

            public Complex( int arg1, string arg2 )
            {
                this.Arg1 = arg1;
                this.Arg2 = arg2;
            }

            public Complex( string arg2, int arg1 )
            {
                this.Arg1 = arg1;
                this.Arg2 = arg2;
            }
        }

        #region Arrays

        [TestMethod]
        public void CreateEmptyPrimitiveArrayStrongTyped()
        {
            var instance = InstanceFactory.CreateObject<int[]>();
            Assert.IsTrue( instance.Length == 0 );
        }

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
        public void CreateEmptyComplexArrayStrongTyped()
        {
            var instance = InstanceFactory.CreateObject<Complex[]>();
            Assert.IsTrue( instance.Length == 0 );
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

        [TestMethod]
        public void CreateComplexMultiplesCtors()
        {
            var instance1 = InstanceFactory.CreateObject<Complex>( 11 );
            Assert.IsTrue( instance1.Arg1 == 11 );
            Assert.IsTrue( instance1.Arg2 == null );

            //different number of params
            var instance2 = InstanceFactory.CreateObject<Complex>( 11, "second arg" );
            Assert.IsTrue( instance2.Arg1 == 11 );
            Assert.IsTrue( instance2.Arg2 == "second arg" );

            //same number of params but different type (different order actually)
            var instance3 = InstanceFactory.CreateObject<Complex>( "second arg", 11 );
            Assert.IsTrue( instance3.Arg1 == 11 );
            Assert.IsTrue( instance3.Arg2 == "second arg" );
        }
    }

    [TestClass]
    public class InstanceFactoryWeakTypedTests
    {
        private class Complex
        {
            public int Arg1 { get; }
            public string Arg2 { get; }

            public Complex() { }

            public Complex( int arg1 )
            {
                this.Arg1 = arg1;
            }

            public Complex( int arg1, string arg2 )
            {
                this.Arg1 = arg1;
                this.Arg2 = arg2;
            }

            public Complex( string arg2, int arg1 )
            {
                this.Arg1 = arg1;
                this.Arg2 = arg2;
            }
        }

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

        [TestMethod]
        public void CreateComplexMultiplesCtors()
        {
            var instance1 = (Complex)InstanceFactory.CreateObject( typeof( Complex ), 11 );
            Assert.IsTrue( instance1.Arg1 == 11 );
            Assert.IsTrue( instance1.Arg2 == null );

            //different number of params
            var instance2 = (Complex)InstanceFactory.CreateObject( typeof( Complex ), 11, "second arg" );
            Assert.IsTrue( instance2.Arg1 == 11 );
            Assert.IsTrue( instance2.Arg2 == "second arg" );

            //same number of params but different type (different order actually)
            var instance3 = (Complex)InstanceFactory.CreateObject( typeof( Complex ), "second arg", 11 );
            Assert.IsTrue( instance3.Arg1 == 11 );
            Assert.IsTrue( instance3.Arg2 == "second arg" );
        }
    }
}

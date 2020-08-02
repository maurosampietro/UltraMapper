using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InterfacesTests
    {
        private interface I { int MyProperty { get; set; } }
        private class IA : I { public int MyProperty { get; set; } }
        private class IB : I { public int MyProperty { get; set; } public int MyProperty2 { get; set; } }
        private class MyType { public I Interface { get; set; } }

        private interface I2 { int MyProperty { get; set; } }
        private class IA2 : I2 { public int MyProperty { get; set; } }
        private class IB2 : I2 { public int MyProperty { get; set; } }
        private class MyType2 { public I2 Interface { get; set; } }

        private interface I3 : I { }
        private class IB3 : I3 { public int MyProperty { get; set; } }
        private class MyType3 { public I3 Interface { get; set; } }

        [TestMethod]
        public void DeepCopyWithInterface()
        {
            var source = new MyType() { Interface = new IB() { MyProperty = 1, MyProperty2 = 2 } };

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsTrue( !Object.ReferenceEquals( source.Interface, target.Interface ) );
        }

        [TestMethod]
        public void MappingWithInterfaceToParentInterface()
        {
            var source = new MyType3()
            {
                Interface = new IB3()
                {
                    MyProperty = 1
                }
            };

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<MyType>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsTrue( !Object.ReferenceEquals( source.Interface, target.Interface ) );
        }

        [TestMethod]
        public void MappingWithInterfaceProvideContructor()
        {
            var source = new MyType() { Interface = new IB() { MyProperty = 1 } };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<I, I2>( () => new IA2() );
            } );

            var target = ultraMapper.Map<MyType2>( source );
            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsTrue( !Object.ReferenceEquals( source.Interface, target.Interface ) );
        }

        [TestMethod]
        public void CollectionOfInterfaceElementToCollectionPrimitiveElement()
        {
            var source = new List<I>()
            {
                new IA(){ MyProperty = 0 },
                new IB(){ MyProperty = 1 }
            };

            var target = new List<string>();

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<IA, string>( a => a.MyProperty.ToString() );
                cfg.MapTypes<IB, string>( b => b.MyProperty.ToString() );
            } );

            mapper.Map( source, target );
        }

        [TestMethod]
        public void CollectionOfInterfaceElementToCollectionOfComplexElement()
        {
            var source = new List<I>()
            {
                new IA(){ MyProperty = 0 },
                new IB(){ MyProperty = 1 }
            };

            var target = new List<IA2>();

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<IA, IA2>( a => new IA2() { MyProperty = a.MyProperty } );
                cfg.MapTypes<IB, IB2>( b => new IB2() { MyProperty = b.MyProperty } );
            } );
            
            mapper.Map( source, target );
        }

        [TestMethod]
        public void CollectionOfInterfaceElementToCollectionOfOtherInterfaceElement()
        {
            var source = new List<I>()
            {
                new IA(){ MyProperty = 0 },
                new IB(){ MyProperty = 1 }
            };

            var target = new List<I2>();
            var mapper = new Mapper();

            //Because we don't know what concrete type to create on target
            Assert.ThrowsException<Exception>( () => mapper.Map( source, target ) );
        }
    }
}

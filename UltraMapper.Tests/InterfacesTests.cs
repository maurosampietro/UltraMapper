using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
    }
}

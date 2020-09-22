using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InterfacesTests
    {
        private interface I { int MyProperty { get; set; } }
        private class TypeAImplementingI : I { public int MyProperty { get; set; } }
        private class TypeBImplementingI : I { public int MyProperty { get; set; } public int MyProperty2 { get; set; } }
        private class MyType { public I Interface { get; set; } }

        private interface I2 { int MyProperty { get; set; } }
        private class TypeAImplementingI2 : I2 { public int MyProperty { get; set; } }
        private class TypeBImplementingI2 : I2 { public int MyProperty { get; set; } }
        private class MyType2 { public I2 Interface { get; set; } }

        [TestMethod]
        public void DeepCopyWithInterface()
        {
            var source = new MyType() { Interface = new TypeBImplementingI() { MyProperty = 1, MyProperty2 = 2 } };

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsTrue( !Object.ReferenceEquals( source.Interface, target.Interface ) );
        }

        [TestMethod]
        public void CollectionDeepCopyThroughInterface()
        {
            //I'd expect to create exactly the same type per element on the target and cast to the interface 

            var source = new List<I>()
            {
                new TypeAImplementingI() { MyProperty = 1 },
                new TypeBImplementingI() { MyProperty = 1, MyProperty2 = 2 }
            };

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void MappingWithInterfaceToAnotherInterface()
        {
            var source = new MyType2()
            {
                Interface = new TypeAImplementingI2()
                {
                    MyProperty = 1
                }
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<I2, I>( () => new TypeAImplementingI() );
            } );

            var target = ultraMapper.Map<MyType>( source );
            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsTrue( !Object.ReferenceEquals( source.Interface, target.Interface ) );
        }

        [TestMethod]
        public void MappingWithInterfaceToAnotherInterfaceNoCtorThrows()
        {
            var source = new MyType2()
            {
                Interface = new TypeAImplementingI2()
                {
                    MyProperty = 1
                }
            };

            Assert.ThrowsException<Exception>( () =>
            {
                var ultraMapper = new Mapper();
                var target = ultraMapper.Map<MyType>( source );
                ultraMapper.VerifyMapperResult( source, target );
            } );
        }

        [TestMethod]
        public void MappingWithInterfaceProvideContructor()
        {
            var source = new MyType() { Interface = new TypeBImplementingI() { MyProperty = 1 } };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<I, I2>( () => new TypeAImplementingI2() );
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
                new TypeAImplementingI(){ MyProperty = 1 },
                new TypeBImplementingI(){ MyProperty = 1, MyProperty2 = 2 }
            };

            var target = new List<I2>();

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<I, I2>( () => new TypeBImplementingI2() );
            } );

            mapper.Map( source, target );
            Assert.IsTrue( mapper.VerifyMapperResult( source, target ) );
        }

        [TestMethod]
        public void CollectionOfInterfaceElementToCollectionPrimitiveElementWithConverter()
        {
            var source = new List<I>()
            {
                new TypeAImplementingI(){ MyProperty = 1 },
                new TypeBImplementingI(){ MyProperty = 1, MyProperty2 = 2 }
            };

            var target = new List<string>();

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<TypeAImplementingI, string>( a => a.MyProperty.ToString() );
                cfg.MapTypes<TypeBImplementingI, string>( b => b.MyProperty.ToString() );
            } );

            mapper.Map( source, target );
            Assert.IsTrue( mapper.VerifyMapperResult( source, target ) );
        }

        [TestMethod]
        public void CollectionOfInterfaceElementToCollectionOfComplexElement()
        {
            var source = new List<I>()
            {
                new TypeAImplementingI(){ MyProperty = 0 },
                new TypeBImplementingI(){ MyProperty = 1 }
            };

            var target = new List<TypeAImplementingI2>();

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<TypeAImplementingI, TypeAImplementingI2>( a => new TypeAImplementingI2() { MyProperty = a.MyProperty } );
                cfg.MapTypes<TypeBImplementingI, TypeBImplementingI2>( b => new TypeBImplementingI2() { MyProperty = b.MyProperty } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( mapper.VerifyMapperResult( source, target ) );
        }

        [TestMethod]
        public void CollectionOfInterfaceElementToCollectionOfOtherInterfaceElement()
        {
            var source = new List<I>()
            {
                new TypeAImplementingI(){ MyProperty = 0 },
                new TypeBImplementingI(){ MyProperty = 1 }
            };

            var target = new List<I2>();
            var mapper = new Mapper();

            //Because we don't know what concrete type to create on target
            Assert.ThrowsException<ArgumentException>( () => mapper.Map( source, target ) );
        }

        [TestMethod]
        public void CollectionBehindInterface()
        {
            var source = new List<string>() { "a", "b", "c" };
            IEnumerable<string> target = new List<string>();
            var mapper = new Mapper();

            mapper.Map( source, target );
            Assert.IsTrue( mapper.VerifyMapperResult( source, target ) );
        }

        //public void Cases()
        //{
        //    var mapper = new Mapper();
        //    I source = new TypeAImplementingI() { MyProperty = 11 };
        //    mapper.Map<I, I2>(source);
        //    mapper.Map<IEnumerable<I>, IEnumerable<I2>>();
        //    mapper.Map<IEnumerable<I>, List<I2>>();


        //}
    }
}

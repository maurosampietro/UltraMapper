using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ObjectReferenceTests
    {
        private class OuterType
        {
            public InnerType NullInnerType { get; set; }
            public InnerType InnerType { get; set; }
            public List<int> PrimitiveList { get; set; }
            public List<InnerType> ComplexList { get; set; }

            public string String { get; set; }
        }

        private class OuterTypeDto
        {
            public InnerTypeDto NullInnerTypeDto { get; set; }
            public InnerTypeDto InnerTypeDto { get; set; }

            public OuterTypeDto()
            {
                this.NullInnerTypeDto = new InnerTypeDto();
                this.InnerTypeDto = new InnerTypeDto();
            }
        }

        private class InnerType
        {
            public string A { get; set; }
            public string B { get; set; }

            public OuterType C { get; set; }
        }

        private class InnerTypeDto
        {
            public string A { get; set; }
            public string B { get; set; }

            public OuterType C { get; set; }
        }

        [TestMethod]
        public void ReferenceBackedByObjectTypeTest1()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            object source = strongTypeSource;
            object target = new OuterType();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ReferenceBackedByObjectTypeTest2()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            object source = strongTypeSource;
            var target = new OuterType();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ReferenceBackedByObjectTypeTest3()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            object source = strongTypeSource;

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map( source );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        [Ignore]
        public void ReferenceBackedByObjectTypeTest4()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            object source = strongTypeSource;

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<object>( source );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        [Ignore]
        public void ReferenceBackedByObjectTypeTest5()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            object source = strongTypeSource;

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<object>( source );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        [Ignore]
        public void ReferenceBackedByObjectTypeTest6()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            OuterType source = strongTypeSource;

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<object>( source );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }



        private class A
        {
            public ICollection<A> Collection { get; set; }
        }

        private class B
        {
           public ICollection<B> Collection { get; set; }
        }

        [TestMethod]
        public void ObjectToObject()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var strongTypeSource = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            strongTypeSource.InnerType.C = strongTypeSource;

            object source = strongTypeSource;

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map<OuterType>( source );

            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.GetType() == target.GetType() );
            Assert.IsTrue( target.GetType() == typeof( OuterType ) );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

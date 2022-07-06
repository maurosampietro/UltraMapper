using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ReferenceTypeTests
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
        public void ReferenceSimpleTest()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var source = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            source.InnerType.C = source;

            var target = new OuterType();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( source.InnerType != target.InnerType );
            Assert.IsTrue( source.String == target.String );
            Assert.IsTrue( source.PrimitiveList.SequenceEqual( target.PrimitiveList ) );
            Assert.IsTrue( source.ComplexList.Count == target.ComplexList.Count );
            Assert.IsTrue( target.ComplexList.First() == target.InnerType );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UseTargetInstanceIfNotNull()
        {
            var innerType = new InnerType() { A = "fadadfsadsffsd" };

            var source = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 0, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType }
            };

            var target = new OuterType()
            {
                InnerType = new InnerType(),
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType }
            };

            var beforeMapInnerType = target.InnerType;
            var beforeMapPrimitiveList = target.PrimitiveList;
            var beforeMapComplexList = target.ComplexList;

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior =
                    ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( Object.ReferenceEquals( target.InnerType, beforeMapInnerType ) );
            Assert.IsTrue( Object.ReferenceEquals( target.PrimitiveList, beforeMapPrimitiveList ) );
            Assert.IsTrue( Object.ReferenceEquals( target.ComplexList, beforeMapComplexList ) );
        }

        [TestMethod]
        public void CreateNewInstance()
        {
            var innerType = new InnerType() { A = "fadadfsadsffsd" };

            var source = new OuterType()
            {
                PrimitiveList = Enumerable.Range( 0, 10 ).ToList(),

                InnerType = new InnerType()
                {
                    A = "a",
                    B = "b"
                },

                ComplexList = new List<InnerType>()
                {
                    new InnerType() { A = "a", B = "b", },
                    new InnerType() { A = "c", B = "d", }
                }
            };

            var target = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList()
            };

            var primitiveList = target.PrimitiveList;

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.ReferenceBehavior =
                    ReferenceBehaviors.CREATE_NEW_INSTANCE;
            } );

            ultraMapper.Map( source, target, null, ReferenceBehaviors.CREATE_NEW_INSTANCE );
            Assert.IsFalse( Object.ReferenceEquals( target.InnerType, innerType ) );
            Assert.IsFalse( Object.ReferenceEquals( target.PrimitiveList, primitiveList ) );
        }
    }
}

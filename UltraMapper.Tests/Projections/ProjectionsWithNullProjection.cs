using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.Tests.Projections
{
    [TestClass]
    public class ProjectionsWithNullProjection
    {
        [TestMethod]
        public void DefaultConverter_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = DefaultMapper.Map<D>( aObject );
            Assert.AreEqual( 2, result.NullableId );
        }

        [TestMethod]
        public void DefaultConverter_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };
            var result = DefaultMapper.Map<D>( aObject );
            Assert.AreEqual( 0, result.NullableId );
        }

        [TestMethod]
        public void DefaultConverter_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = DefaultMapper.Map<D>( aObject );
            Assert.IsNull( result.NullableId );
        }

        [TestMethod]
        public void DefaultConverter_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = DefaultMapper.Map<D>( aObject );
            Assert.IsNull( result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = CustomConverionMapper.Map<D>( aObject );
            Assert.AreEqual( 2, result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConverionMapper.Map<D>( aObject );
            Assert.AreEqual( 0, result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConverionMapper.Map<D>( aObject );
            Assert.AreEqual( -1, result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConverionMapper.Map<D>( aObject );
            Assert.AreEqual( -1, result.NullableId );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = CustomConverionMapper.Map<F>( aObject );
            Assert.AreEqual( "2", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConverionMapper.Map<F>( aObject );
            Assert.AreEqual( "0", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConverionMapper.Map<F>( aObject );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConverionMapper.Map<F>( aObject );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = CustomConverionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "My val 2", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConverionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "My val 0", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConverionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "MY NULL", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConverionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "MY NULL", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_ExistingObject_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = new F();
            CustomConverionMapper.Map( aObject, result );
            Assert.AreEqual( "2", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_ExistingObject_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };
            var result = new F();
            CustomConverionMapper.Map( aObject, result );
            Assert.AreEqual( "0", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_ExistingObject_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = new F();
            CustomConverionMapper.Map( aObject, result );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_ExistingObject_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = new F();
            CustomConverionMapper.Map( aObject, result );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        private Mapper DefaultMapper => new Mapper( cfg =>
            {
                cfg.MapTypes<A, D>()
                 .MapMemberNullProjection( a => a.BProp.CProp.Id, d => d.NullableId );
            }
        );

        private Mapper CustomConverionMapper => new Mapper( cfg =>
        {
            cfg.MapTypes<A, D>()
             .MapMemberNullProjection( a => a.BProp.CProp.Id, d => d.NullableId, s => s ?? -1 );

            cfg.MapTypes<A, F>()
             .MapMemberNullProjection( a => a.BProp.CProp.Id, f => f.ConvertTo, s => s.HasValue ? s.ToString() : "NULL" );

            cfg.MapTypes<A, ParentHolder>()
              .MapMemberNullProjection( a => a.BProp.CProp.Id, p => p.ConvertTo );

            cfg.MapTypes<long?, TargetTypeToMap>( s => new TargetTypeToMap { ConvertTo = s.HasValue ? $"My val {s}" : "MY NULL" } );
        }
        );


        public class A
        {
            public B BProp { get; set; }
        }

        public class B
        {
            public C CProp { get; set; }
        }

        public class C
        {
            public long Id { get; set; }
        }

        public class D
        {
            public long? NullableId { get; set; }
        }

        public class F
        {
            public string ConvertTo { get; set; }
        }

        public class TargetTypeToMap
        {
            public string ConvertTo { get; set; }
        }

        public class ParentHolder
        {
            public TargetTypeToMap ConvertTo { get; set; }
        }
    }
}

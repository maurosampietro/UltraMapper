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
            var result = CustomConversionMapper.Map<D>( aObject );
            Assert.AreEqual( 2, result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConversionMapper.Map<D>( aObject );
            Assert.AreEqual( 0, result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConversionMapper.Map<D>( aObject );
            Assert.AreEqual( -1, result.NullableId );
        }

        [TestMethod]
        public void CustomConverter_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConversionMapper.Map<D>( aObject );
            Assert.AreEqual( -1, result.NullableId );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = CustomConversionMapper.Map<F>( aObject );
            Assert.AreEqual( "2", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConversionMapper.Map<F>( aObject );
            Assert.AreEqual( "0", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConversionMapper.Map<F>( aObject );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        [TestMethod]
        public void CustomConverterOtherType_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConversionMapper.Map<F>( aObject );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = CustomConversionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "My val 2", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_WithMemberConverters_MapTestPropHasValue()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 2 } }
            };
            var result = CustomConversionMapperUsingMemberConverters.Map<ParentHolder>( aObject );
            Assert.AreEqual( "My val 2", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConversionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "My val 0", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_WithMemberConverters_MapTestPropDefault()
        {
            var aObject = new A
            {
                BProp = new B { CProp = new C { Id = 0 } }
            };

            var result = CustomConversionMapperUsingMemberConverters.Map<ParentHolder>( aObject );
            Assert.AreEqual( "My val 0", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConversionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "MY NULL", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_WithMemberConverters_MapTestChain1Null()
        {
            var aObject = new A
            {
                BProp = null
            };
            var result = CustomConversionMapperUsingMemberConverters.Map<ParentHolder>( aObject );
            Assert.AreEqual( "MY NULL", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConversionMapper.Map<ParentHolder>( aObject );
            Assert.AreEqual( "MY NULL", result.ConvertTo.ConvertTo );
        }

        [TestMethod]
        public void CustomExternalConverter_WithMemberConverters_MapTestChain2Null()
        {
            var aObject = new A
            {
                BProp = new B { CProp = null }
            };
            var result = CustomConversionMapperUsingMemberConverters.Map<ParentHolder>( aObject );
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
            CustomConversionMapper.Map( aObject, result );
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
            CustomConversionMapper.Map( aObject, result );
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
            CustomConversionMapper.Map( aObject, result );
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
            CustomConversionMapper.Map( aObject, result );
            Assert.AreEqual( "NULL", result.ConvertTo );
        }

        private Mapper DefaultMapper => new Mapper( cfg =>
            {
                cfg.MapTypes<A, D>()
                 .MapMember( a => a.BProp.CProp.Id, d => d.NullableId );
            }
        );

        private Mapper CustomConversionMapperUsingMemberConverters => new Mapper( cfg =>
        {
            cfg.MapTypes<A, D>()
             .MapMember<long?, long?>( a => a.BProp.CProp.Id, d => d.NullableId, converter: ( refTracker, s ) => s ?? -1 );

            cfg.MapTypes<A, F>()
             .MapMember<long?, string>( a => a.BProp.CProp.Id, f => f.ConvertTo, converter: ( refTracker, s ) => s.HasValue ? s.ToString() : "NULL" );

            cfg.MapTypes<A, ParentHolder>()
              .MapMember<long?, TargetTypeToMap>( a => a.BProp.CProp.Id, p => p.ConvertTo, converter: ( refTracker, s ) => new TargetTypeToMap { ConvertTo = s.HasValue ? $"My val {s}" : "MY NULL" } );
        } );

        private Mapper CustomConversionMapper => new Mapper( cfg =>
        {
            cfg.MapTypes<A, D>()
             .MapMember<long?, long?>( a => a.BProp.CProp.Id, d => d.NullableId, converter: ( refTracker, s ) => s ?? -1 );

            cfg.MapTypes<A, F>()
             .MapMember<long?,string>( a => a.BProp.CProp.Id, f => f.ConvertTo, converter: (refTracker, s) => s.HasValue ? s.ToString() : "NULL" );

            cfg.MapTypes<A, ParentHolder>()
              .MapMember( a => a.BProp.CProp.Id, p => p.ConvertTo );

            cfg.MapTypes<long, TargetTypeToMap>( s => new TargetTypeToMap { ConvertTo = $"My val {s}" } );
        } );


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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.Tests.RealworldBugs
{
    [TestClass]
    public class RealworldBug3
    {

        public class A
        {
            public B BProp { get; set; }
            public C CProp { get; set; }
        }

        public class B
        {
            public long Id { get; set; }
        }

        public class C
        {
            public long Id { get; set; }
        }

        public class D
        {
            public long? B_Id { get; set; }
            public long? C_Id { get; set; }
        }


        [TestMethod]
        public void ProjectionMappingFull()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<A, D>()
                 .MapMember( a => a.BProp.Id, d => d.B_Id )
                 .MapMember( a => a.CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = new B { Id = 1 },
                CProp = new C { Id = 2 }
            };

            var result = mapper.Map<D>( aObject );

            Assert.AreEqual( result.B_Id, 1L );
            Assert.AreEqual( result.C_Id, 2L );
        }

        [TestMethod]
        public void ProjectionMappingNull()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<A, D>()
                 .MapMemberNullProjection( a => a.BProp.Id, d => d.B_Id )
                 .MapMemberNullProjection( a => a.CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = null,
                CProp = null
            };

            var result = mapper.Map<D>( aObject );

            Assert.IsNull( result.B_Id );
            Assert.IsNull( result.C_Id );
        }
    }
}
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

        public class E
        {
            public long B_Id { get; set; }
            public long C_Id { get; set; }
        }

        public class F
        {
            public A GetA()
            {
                return AProp;
            }

            public A AProp { get; set; }
        }

        [TestMethod]
        public void ProjectionMappingFullToNullabe()
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

            var result = mapper.Map<D>( aObject);

            Assert.AreEqual( result.B_Id, 1L );
            Assert.AreEqual( result.C_Id, 2L );
        }

        [TestMethod]
        public void ProjectionMappingFullToNullabeMethod()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<F, D>()
                 .MapMember( f => f.GetA().BProp.Id, d => d.B_Id )
                 .MapMember( f => f.GetA().CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = new B { Id = 1 },
                CProp = new C { Id = 2 }
            };
            var fObject = new F { AProp = aObject };
            var result = mapper.Map<D>( fObject );
            Assert.AreEqual( result.B_Id, 1L );
            Assert.AreEqual( result.C_Id, 2L );
        }



        [TestMethod]
        public void ProjectionMappingNullToNullable()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<A, D>()
                 .MapMember( a => a.BProp.Id, d => d.B_Id )
                 .MapMember( a => a.CProp.Id, d => d.C_Id );
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

        [TestMethod]
        public void ProjectionMappingNullToNullableMethod()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<F, D>()
                 .MapMember( f => f.GetA().BProp.Id, d => d.B_Id )
                 .MapMember( f => f.GetA().CProp.Id, d => d.C_Id );
            } );
            var fObject = new F();
            var result = mapper.Map<D>( fObject );
            Assert.IsNull( result.B_Id );
            Assert.IsNull( result.C_Id );
        }


        [TestMethod]
        public void ProjectionMappingFull()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<A, E>()
                 .MapMember( a => a.BProp.Id, d => d.B_Id )
                 .MapMember( a => a.CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = new B { Id = 1 },
                CProp = new C { Id = 2 }
            };

            var result = mapper.Map<E>( aObject );

            Assert.AreEqual( result.B_Id, 1L );
            Assert.AreEqual( result.C_Id, 2L );
        }

        [TestMethod]
        public void ProjectionMappingDefault()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<A, E>()
                 .MapMember( a => a.BProp.Id, d => d.B_Id )
                 .MapMember( a => a.CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = null,
                CProp = null
            };

            var result = mapper.Map<E>( aObject );

            Assert.AreEqual( 0, result.B_Id );
            Assert.AreEqual( 0, result.C_Id );
        }

        [TestMethod]
        public void ProjectionMappingFullMethod()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<F, E>()
                 .MapMember( a => a.GetA().BProp.Id, d => d.B_Id )
                 .MapMember( a => a.GetA().CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = new B { Id = 1 },
                CProp = new C { Id = 2 }
            };
            var fObject = new F { AProp = aObject };
            var result = mapper.Map<E>( fObject );

            Assert.AreEqual( result.B_Id, 1L );
            Assert.AreEqual( result.C_Id, 2L );
        }

        [TestMethod]
        public void ProjectionMappingDefaultMethod()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<F, E>()
                 .MapMember( a => a.GetA().BProp.Id, d => d.B_Id )
                 .MapMember( a => a.GetA().CProp.Id, d => d.C_Id );
            } );

            var aObject = new A
            {
                BProp = null,
                CProp = null
            };
            var fObject = new F { AProp = aObject };
            var result = mapper.Map<E>( fObject );

            Assert.AreEqual( 0, result.B_Id );
            Assert.AreEqual( 0, result.C_Id );
        }


    }
}

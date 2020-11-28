using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.Tests
{
    [TestClass]
    public class RealworldBug2
    {
        public class SourceClass
        {
            public string Id { get; set; }
        }

        public class TargetClass
        {
            public Guid Id { get; set; }
        }

        [TestMethod]
        public void StringToGuidTypeMapping()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<string, Guid>( str => Guid.Parse( str ) );
            } );

            var source = new SourceClass() { Id = Guid.NewGuid().ToString() };
            var result = mapper.Map<TargetClass>( source );

            bool isResultOk = mapper.VerifyMapperResult( source, result );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void StringToGuidMemberMapping()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<SourceClass, TargetClass>()
                    .MapMember( s => s.Id, t => t.Id, str => Guid.Parse( str ) );
            } );

            var source = new SourceClass() { Id = Guid.NewGuid().ToString() };
            var result = mapper.Map<TargetClass>( source );

            bool isResultOk = mapper.VerifyMapperResult( source, result );
            Assert.IsTrue( isResultOk );
        }
    }
}

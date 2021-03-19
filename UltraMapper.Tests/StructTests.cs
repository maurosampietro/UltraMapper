using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.Tests
{
    [TestClass]
    public class StructTests
    {
        private class Test
        {
            public DateTime DateTime { get; set; }
                = new DateTime( 1988, 05, 29 );
        }

        private struct StructTest
        {
            public DateTime DateTime { get; set; }
        }

        [TestMethod]
        public void DateTimeDirectTypeMapping()
        {
            DateTime dateTime = new DateTime( 2017, 03, 13 );

            var mapper = new Mapper();
            mapper.Map( dateTime, out DateTime clone );

            var mapping = mapper.Config[ typeof( DateTime ), typeof( DateTime ) ];

            Assert.IsTrue( dateTime == clone );
            Assert.IsTrue( !Object.ReferenceEquals( dateTime, clone ) );
            Assert.IsTrue( mapping.Mapper is StructMapper ); //convert mapper can get the job done as well but is slower
        }

        [TestMethod]
        public void DateTimeMemberMapping()
        {
            var test = new Test();
            var mapper = new Mapper();

            var clone = mapper.Map( test );

            Assert.IsTrue( test.DateTime == clone.DateTime );
            Assert.IsTrue( !Object.ReferenceEquals( test.DateTime, clone.DateTime ) );
        }

        [TestMethod]
        public void ClassToStructMapping()
        {
            var mapper = new Mapper();

            var source = new Test();
            var target = new StructTest();

            mapper.Map( source, out target );

            var result = mapper.VerifyMapperResult( source, target );
            Assert.IsTrue( result );
        }

        [TestMethod]
        public void StructToClassMapping()
        {
            var mapper = new Mapper();

            var source = new StructTest()
            {
                DateTime = new DateTime( 2013, 12, 18 )
            };

            var target = mapper.Map<Test>( source );

            var result = mapper.VerifyMapperResult( source, target );
            Assert.IsTrue( result );
        }
    }
}

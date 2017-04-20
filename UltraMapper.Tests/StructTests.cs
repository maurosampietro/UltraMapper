using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            DateTime clone;

            var mapper = new UltraMapper();
            mapper.Map( dateTime, out clone );

            Assert.IsTrue( dateTime == clone );
            Assert.IsTrue( !Object.ReferenceEquals( dateTime, clone ) );
        }

        [TestMethod]
        public void DateTimeMemberMapping()
        {
            var test = new Test();
            var mapper = new UltraMapper();

            var clone = mapper.Map( test );

            Assert.IsTrue( test.DateTime == clone.DateTime );
            Assert.IsTrue( !Object.ReferenceEquals( test.DateTime, clone.DateTime ) );
        }

        [TestMethod]
        public void ClassToStructMapping()
        {
            var mapper = new UltraMapper();

            var source = new Test();
            var target = new StructTest();

            mapper.Map( source, out target );

            var result = mapper.VerifyMapperResult( source, target );
            Assert.IsTrue( result );
        }

        [TestMethod]
        public void StructToClassMapping()
        {
            var mapper = new UltraMapper();

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

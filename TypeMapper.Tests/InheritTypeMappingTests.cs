using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Tests
{
    [TestClass]
    public class InheritTypeMappingTests
    {
        public class TestClass
        {
            public bool Boolean { get; set; }
            public string String { get; set; }
        }

        [TestMethod]
        public void InheritMapping()
        {
            var source = new TestClass();
            var target = new TestClass();

            var typeMapper = new TypeMapper( cfg =>
            {
                cfg.MapTypes<bool, string>( null, b => b ? "1" : "0" );

                cfg.MapTypes<TestClass, TestClass>()
                    .MapProperty( a => a.Boolean, y => y.String );
            } );

            typeMapper.Map( source, target );

            Assert.IsTrue( source.Boolean ? target.String == "1"
                : target.String == "0" );
        }
    }
}

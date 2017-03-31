using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InheritTypeMappingTests
    {
        public class TestClass
        {
            public bool Boolean { get; set; }
            public string String { get; set; }
            public List<string> Strings { get; set; } = new List<string>();
            public List<bool> Booleans { get; set; } = new List<bool>();
        }

        [TestMethod]
        public void InheritMapping()
        {
            var source = new TestClass();
            source.Strings.Clear();
            source.Booleans.Clear();

            source.Booleans.Add( true );
            source.Booleans.Add( false );

            var target = new TestClass();

            var ultraMapper = new UltraMapper( cfg =>
            {
                cfg.MapTypes<bool, string>( null, b => b ? "1" : "0" );

                cfg.MapTypes<TestClass, TestClass>()
                    .MapMember( a => a.Boolean, y => y.String )
                    .MapMember( a => a.Booleans, y => y.Strings );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Boolean ? target.String == "1"
                : target.String == "0" );

            Assert.IsTrue( target.Strings.Contains( "1" ) &&
                target.String.Contains( "0" ) );
        }
    }
}

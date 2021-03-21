using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable( EditorBrowsableState.Never )]
    public class IsExternalInit { }
}

namespace UltraMapper.Tests
{
    [TestClass]
    public class Records
    {
        public record Record
        {
            public string Value { get; init; }
        }

        public class OtherObject
        {
            public string Value { get; set; }
        }

        [TestMethod]
        public void BasicTest()
        {
            var mapper = new Mapper();

            var otherObject = new OtherObject() { Value = "ciao" };

            var map1 = mapper.Map<Record>( otherObject );
            var map2 = mapper.Map<Record>( otherObject );

            Assert.IsTrue( map1.Equals( map2 ) );

            var r1 = new Record { Value = otherObject.Value };
            var r2 = new Record { Value = otherObject.Value };

            Assert.IsTrue( r1.Equals( r2 ) );
        }
    }
}

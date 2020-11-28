using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.Tests
{
    [TestClass]
    public class CircularReferenceType
    {
        public class OuterType
        {
            public class InnerType
            {
                public string A { get; set; }
                public string B { get; set; }
                public InnerType Inner { get; set; }
            }

            public InnerType Move { get; set; }
        }

        [TestMethod]
        public void NestingAndReferringTheSameType()
        {
            var source = new OuterType()
            {
                Move = new OuterType.InnerType
                {
                    A = "a",
                    B = "b",
                    Inner = new OuterType.InnerType()
                    {
                        A = "c",
                        B = "b"
                    }
                }
            };

            var ultraMapper = new Mapper();
            var target = ultraMapper.Map( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

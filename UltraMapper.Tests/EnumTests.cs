using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.Tests
{
    [TestClass]
    public class EnumTests
    {
        public enum Types
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }

        [TestMethod]
        public void StringToEnum()
        {
            var ultraMapper = new Mapper();

            string source = Types.Value1.ToString();
            Types target = Types.Value3;

            ultraMapper.Map( source, out target );
            Assert.IsTrue( target == Types.Value1 );
        }

        [TestMethod]
        public void EnumToEnum()
        {
            var ultraMapper = new Mapper();

            string source = Types.Value1.ToString();
            Types target = Types.Value3;

            ultraMapper.Map( Types.Value1, out target );
            Assert.IsTrue( target == Types.Value1 );
        }
    }
}

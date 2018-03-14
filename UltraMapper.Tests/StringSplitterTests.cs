using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class StringSplitterTests
    {
        [TestMethod]
        public void Split1()
        {
            var splitter = new StringSplitter( StringSplittingRules.PascalCase );
            var result = splitter.Split( "ABCDEFG" ).ToList();

            var isTrue = result.SequenceEqual( new[] {
                "A", "B", "C", "D", "E", "F", "G" } );

            Assert.IsTrue( isTrue );
        }

        [TestMethod]
        public void Split2()
        {
            var splitter = new StringSplitter( StringSplittingRules.PascalCase );
            var result = splitter.Split( "AxBxCxDxExFxGx" ).ToList();

            var isTrue = result.SequenceEqual( new[] {
                "Ax", "Bx", "Cx", "Dx", "Ex", "Fx", "Gx" } );

            Assert.IsTrue( isTrue );
        }

        [TestMethod]
        public void Split3()
        {
            var splitter = new StringSplitter( StringSplittingRules.PascalCase );
            var result = splitter.Split( "xAxBxCxDxExFxGxe" ).ToList();

            var isTrue = result.SequenceEqual( new[] {
                "x","Ax", "Bx", "Cx", "Dx", "Ex", "Fx", "Gxe" } );

            Assert.IsTrue( isTrue );
        }

        [TestMethod]
        public void Split4()
        {
            var splitter = new StringSplitter( StringSplittingRules.SnakeCase );
            var result = splitter.Split( "xAxBxCxDx_ExFxGxe" ).ToList();

            var isTrue = result.SequenceEqual( new[] { "xAxBxCxDx", "ExFxGxe" } );

            Assert.IsTrue( isTrue );
        }
    }
}

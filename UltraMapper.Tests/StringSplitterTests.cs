using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class StringSplitterTests
    {
        [TestMethod]
        public void Split1()
        {
            var splitter = new StringSplitter( new PascalCaseSplittingRule() );
            var result = splitter.Split( "ABCDEFG" ).ToList();

            var isTrue = result.SequenceEqual( new List<string>() {
                "A", "B", "C", "D", "E", "F", "G" } );

            Assert.IsTrue( isTrue );
        }

        [TestMethod]
        public void Split2()
        {
            var splitter = new StringSplitter( new PascalCaseSplittingRule() );
            var result = splitter.Split( "AxBxCxDxExFxGx" ).ToList();

            var isTrue = result.SequenceEqual( new List<string>() {
                "Ax", "Bx", "Cx", "Dx", "Ex", "Fx", "Gx" } );

            Assert.IsTrue( isTrue );
        }

        [TestMethod]
        public void Split3()
        {
            var splitter = new StringSplitter( new PascalCaseSplittingRule() );
            var result = splitter.Split( "xAxBxCxDxExFxGxe" ).ToList();

            var isTrue = result.SequenceEqual( new List<string>() {
                "x","Ax", "Bx", "Cx", "Dx", "Ex", "Fx", "Gxe" } );

            Assert.IsTrue( isTrue );
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Tests
{
    [TestClass]
    public class MemberExtractionTests
    {
        private class FirstLevel
        {
            public string A { get; set; }
            public string field;

            public SecondLevel SecondLevel { get; set; }

            public SecondLevel GetSecond() { return SecondLevel; }
        }

        private class SecondLevel
        {
            public string A { get; set; }
            public string field;

            public ThirdLevel ThirdLevel { get; set; }

            public ThirdLevel GetThird() { return this.ThirdLevel; }
        }

        private class ThirdLevel
        {
            public string A { get; set; }
            public string field;
        }

        [TestMethod]
        public void ExtractPropertyInfo()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, string>> func = ( fl ) => fl.A;

            var expectedMember = typeof( FirstLevel )
                .GetMember( nameof( FirstLevel.A ) )[ 0 ];

            var extractedMember = func.ExtractMember();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractFieldInfo()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, string>> func = ( fl ) => fl.field;

            var expectedMember = typeof( FirstLevel )
                .GetMember( nameof( FirstLevel.field ) )[ 0 ];

            var extractedMember = func.ExtractMember();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractMethodInfo()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, SecondLevel>> func = ( fl ) => fl.GetSecond();

            var expectedMember = typeof( FirstLevel )
                .GetMember( nameof( FirstLevel.GetSecond ) )[ 0 ];

            var extractedMember = func.ExtractMember();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractNestedMethodInfo()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, ThirdLevel>> func = 
                ( fl ) => fl.SecondLevel.GetThird();

            var expectedMember = typeof( SecondLevel )
                .GetMember( nameof( SecondLevel.GetThird ) )[ 0 ];

            var extractedMember = func.ExtractMember();
            Assert.IsTrue( expectedMember == extractedMember );
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class MemberExtractionTests
    {
        private class FirstLevel
        {
            public string A { get; set; }
            public string field;

            public SecondLevel secondLevel;
            public SecondLevel SecondLevel
            {
                get { return secondLevel; }
                set { secondLevel = value; }
            }

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

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractFieldInfo()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, string>> func = ( fl ) => fl.field;

            var expectedMember = typeof( FirstLevel )
                .GetMember( nameof( FirstLevel.field ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractMethodInfo()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, SecondLevel>> func = ( fl ) => fl.GetSecond();

            var expectedMember = typeof( FirstLevel )
                .GetMember( nameof( FirstLevel.GetSecond ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractNestedMethodViaField()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, ThirdLevel>> func =
                ( fl ) => fl.secondLevel.GetThird();

            var expectedMember = typeof( SecondLevel )
                .GetMember( nameof( SecondLevel.GetThird ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractNestedMethodViaProperty()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, ThirdLevel>> func = 
                ( fl ) => fl.SecondLevel.GetThird();

            var expectedMember = typeof( SecondLevel )
                .GetMember( nameof( SecondLevel.GetThird ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractNestedMethodViaMethod()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, ThirdLevel>> func =
                ( fl ) => fl.GetSecond().GetThird();

            var expectedMember = typeof( SecondLevel )
                .GetMember( nameof( SecondLevel.GetThird ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractNestedPropertyViaMethod()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, ThirdLevel>> func =
                ( fl ) => fl.GetSecond().ThirdLevel;

            var expectedMember = typeof( SecondLevel )
                .GetMember( nameof( SecondLevel.ThirdLevel ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }

        [TestMethod]
        public void ExtractNestedPropertyViaProperty()
        {
            var test = new FirstLevel();
            Expression<Func<FirstLevel, ThirdLevel>> func =
                ( fl ) => fl.SecondLevel.ThirdLevel;

            var expectedMember = typeof( SecondLevel )
                .GetMember( nameof( SecondLevel.ThirdLevel ) )[ 0 ];

            var extractedMember = func.ExtractMember().Last();
            Assert.IsTrue( expectedMember == extractedMember );
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class GetterExpressionBuildingTests
    {
        private class FirstLevel
        {
            public string A { get; set; }
            public string A1 { get; set; }
            public string A2 { get; set; }

            public SecondLevel SecondLevel { get; set; }

            public SecondLevel GetSecond() { return SecondLevel; }
        }

        private class SecondLevel
        {
            public string A { get; set; }
            public string A1 { get; set; }
            public ThirdLevel ThirdLevel { get; set; }

            public ThirdLevel GetThird() { return this.ThirdLevel; }
            public ThirdLevel SetThird( ThirdLevel value ) { return this.ThirdLevel = value; }
        }

        private class ThirdLevel
        {
            public string A { get; set; }
            public string A1 { get; set; }

            public void SetA( string value )
            {
                this.A = value;
            }
        }

        [TestMethod]
        public void BuildGetterWithNullChecks()
        {
            Expression<Func<FirstLevel, string>> selector = t => t.SecondLevel.ThirdLevel.A;

            var accessPath = selector.GetMemberAccessPath();
            var expression = accessPath.GetGetterExpWithNullChecks();
            var functor = (Func<FirstLevel, string>)expression.Compile();

            // LEVEL 1
            var source = new FirstLevel();
            var result = functor( source );

            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );

            // LEVEL 2
            source = new FirstLevel()
            {
                SecondLevel = new SecondLevel()
            };

            result = functor( source );
            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );

            // LEVEL 3
            source = new FirstLevel()
            {
                SecondLevel = new SecondLevel()
                {
                    ThirdLevel = new ThirdLevel()
                }
            };

            result = functor( source );
            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );

            // LEVEL 4
            source = new FirstLevel()
            {
                SecondLevel = new SecondLevel()
                {
                    ThirdLevel = new ThirdLevel()
                    {
                        A = "Ok"
                    }
                }
            };

            result = functor( source );
            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );
        }

        public interface IParsedParam
        {
            string Name { get; set; }
            int Index { get; set; }
        }

        public class SimpleParam : IParsedParam
        {
            public string Name { get; set; }
            public int Index { get; set; }
            public string Value { get; set; }
        }

        [TestMethod]
        public void BuildSelectorWithCast()
        {
            Expression<Func<IParsedParam, string>> selector = s => ((SimpleParam)s).Value;

            var accessPath = selector.GetMemberAccessPath();
            var expression = accessPath.GetGetterExp();
            var functor = (Func<IParsedParam, string>)expression.Compile();
            
            Assert.IsNotNull( functor );
        }
    }
}

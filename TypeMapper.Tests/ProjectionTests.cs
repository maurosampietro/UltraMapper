using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeMapper.Tests
{
    [TestClass]
    public class ProjectionTests
    {
        private class FirstLevel
        {
            public string A { get; set; }
            public SecondLevel SecondLevel { get; set; }

            public SecondLevel GetSecond() { return SecondLevel; }
        }

        private class SecondLevel
        {
            public string A { get; set; }
            public ThirdLevel ThirdLevel { get; set; }

            public ThirdLevel GetThird() { return this.ThirdLevel; }
            public ThirdLevel SetThird( ThirdLevel value ) { return this.ThirdLevel = value; }
        }

        private class ThirdLevel
        {
            public string A { get; set; }
        }

        [TestMethod]
        public void ManualFlattening()
        {
            var source = new FirstLevel()
            {
                A = "first",

                SecondLevel = new SecondLevel()
                {
                    A = "second",

                    ThirdLevel = new ThirdLevel()
                    {
                        A = "third"
                    }
                }
            };

            var target = new FirstLevel();

            var typeMapper = new TypeMapper( cfg =>
            {
                cfg.MapTypes<FirstLevel, FirstLevel>()
                    .MapMember( a => a.SecondLevel.ThirdLevel.A, b => b.A );
                    //.MapMember( a => a.GetSecond().ThirdLevel, ( b, value ) => b.SecondLevel.SetThird( value ) )
            } );

            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

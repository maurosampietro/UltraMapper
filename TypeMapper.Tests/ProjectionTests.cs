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
            var source = new FirstLevel() { SecondLevel = new SecondLevel() { ThirdLevel = new ThirdLevel() } };
            var target = new FirstLevel() { SecondLevel = new SecondLevel() { ThirdLevel = new ThirdLevel() } };

            var typeMapper = new TypeMapper( cfg =>
            {
                cfg.MapTypes<FirstLevel, FirstLevel>()
                    .MapProperty( a => a.SecondLevel.ThirdLevel.A, b => b.A )
                    .MapMethod( a => a.GetSecond().ThirdLevel, ( b, value ) => b.SecondLevel.SetThird( value ) )
                    .MapProperty( a => a.SecondLevel, b => b.SecondLevel );
            } );

            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

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
        }

        private class SecondLevel
        {
            public string A { get; set; }
            public ThirdLevel ThirdLevel { get; set; }
        }

        private class ThirdLevel
        {
            public string A { get; set; }
        }


        [TestMethod]
        public void ManualFlattening()
        {
            var source = new FirstLevel();
            var target = new FirstLevel();

            var typeMapper = new TypeMapper( cfg =>
            {
                cfg.MapTypes<FirstLevel, FirstLevel>()
                    .MapProperty( a => a.SecondLevel.ThirdLevel.A, b => b.A );
            } );

            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

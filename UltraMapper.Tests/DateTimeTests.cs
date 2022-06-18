using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class DateTimeTests
    {
        private class Source
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
        }

        private class Target
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
        }

        [TestMethod]
        public void DateTimeAndString()
        {
            var source = new Source() { Date = new DateTime( 2000, 12, 31 ), DateString = new DateTime( 2000, 1, 1 ).ToString( "yyyyMMdd" ) };
            var target = new Target();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Source, Target>()
                   .MapMember( s => s.Date, t => t.DateString )
                   .MapMember( s => s.DateString, t => t.Date );

                cfg.MapTypes<string, DateTime>( s => DateTime.ParseExact( s, "yyyyMMdd", cfg.Culture ) );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Date.ToString( ultraMapper.Config.Culture ) == target.DateString );
            Assert.IsTrue( DateTime.ParseExact( source.DateString, "yyyyMMdd", ultraMapper.Config.Culture ) == target.Date );
        }
    }
}

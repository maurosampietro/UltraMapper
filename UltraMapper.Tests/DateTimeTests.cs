using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public partial class DateTimeTests
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
            var source = new Source()
            {
                Date = new DateTime( 2000, 12, 31 ),
                DateString = new DateTime( 2000, 1, 1 ).ToString( "yyyyMMdd" )
            };

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

    [TestClass]
    public class DateTimeTests2
    {
        private class Source1
        {
            public DateTime Date1 { get; set; }
            public DateTime Date2 { get; set; }
            public DateTime Date3 { get; set; }
        }

        private class Target
        {
            public string LongDateString { get; set; }
            public string ShortDateString { get; set; }
            public string DefaultFormat { get; set; }
        }

        private class Source2
        {
            public DateTime Date1 { get; set; }
            public DateTime Date2 { get; set; }
            public DateTime Date3 { get; set; }
        }

        [TestMethod]
        public void DifferentFormatsAndConfigInheritance()
        {
            var source1 = new Source1()
            {
                Date1 = new DateTime( 2000, 12, 31 ),
                Date2 = new DateTime( 2001, 12, 31 ),
                Date3 = new DateTime( 2002, 12, 31 )
            };

            var source2 = new Source2()
            {
                Date1 = new DateTime( 2002, 12, 31 ),
                Date2 = new DateTime( 2003, 12, 31 ),
                Date3 = new DateTime( 2002, 12, 31 )
            };

            var target = new Target();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<DateTime, string>( s => "default format" );

                cfg.MapTypes<Source1, Target>()
                    .MapTypeToMember<DateTime, string>( t => t.LongDateString, s => s.ToLongDateString() )
                    .MapTypeToMember<DateTime, string>( t => t.ShortDateString, s => s.ToShortDateString() )
                    .MapMember( s => s.Date1, t => t.LongDateString )
                    .MapMember( s => s.Date2, t => t.ShortDateString )
                    .MapMember( s => s.Date3, t => t.DefaultFormat );

                cfg.MapTypes<Source2, Target>()
                    .MapTypeToMember<DateTime, string>( t => t.LongDateString, s => "long format" )
                    .MapMember( s => s.Date1, t => t.LongDateString )
                    .MapMember( s => s.Date2, t => t.ShortDateString, s => "short format" )
                    .MapMember( s => s.Date3, t => t.DefaultFormat, s => "default format override" );
            } );

            ultraMapper.Map( source1, target );

            Assert.IsTrue( source1.Date1.ToLongDateString() == target.LongDateString );
            Assert.IsTrue( source1.Date2.ToShortDateString() == target.ShortDateString );
            Assert.IsTrue( target.DefaultFormat == "default format" );

            ultraMapper.Map( source2, target );

            Assert.IsTrue( target.LongDateString == "long format" );
            Assert.IsTrue( target.ShortDateString == "short format" );
            Assert.IsTrue( target.DefaultFormat == "default format override" );
        }
    }
}

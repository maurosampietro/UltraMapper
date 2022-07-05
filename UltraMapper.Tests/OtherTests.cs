using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class OtherTests
    {
        public class Case
        {
            public List<Media> Media { get; set; }
        }

        public class Media
        {
            public List<Drawing> Drawings { get; set; }
        }

        public class Drawing
        {
            public BaseItem Item { get; set; }
        }

        public class Container
        {
            public string Data { get; set; }
            public BaseItem Item { get; set; }
        }

        //abstract
        public abstract class BaseItem
        {
            //public double X { get; set; }
            //public double Y { get; set; }
            //public double Width { get; set; }
            //public double Height { get; set; }
        }

        public class DerivedItem : BaseItem
        {
            public Point Point1 { get; set; }
            public string A { get; set; }
            public ObservableCollection<Point> Points { get; set; }
        }

        [TestMethod]
        public void ComplexConverter()
        {
            var source = new Container()
            {
                Data = "ciao",
                Item = new DerivedItem() { A = "test" }
            };

            var target = new Container();

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<Container, Container>()
                   .MapMember( a => a.Data, b => b.Item, a => new DerivedItem() { A = a } );
            } );

            mapper.Map( source, target );

            bool isResultOk = mapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void AbstractBaseClass()
        {
            var source = new Case()
            {
                Media = new List<Media>() { new Media()
                {
                    Drawings = new List<Drawing>()
                    {
                        new Drawing()
                        {
                            Item = new DerivedItem()
                            {
                                A = "a",
                                Point1 = new Point( 1, 1 ),
                            },
                        },

                        new Drawing()
                        {
                            Item = new DerivedItem()
                            {
                                A = "b",
                                Point1 = new Point( 2, 2 ),
                            },
                        }
                    }
                } }
            };

            var mapper = new Mapper( cfg =>
            {
                //cfg.MapTypes<string, BaseItem>( a => (BaseItem)new DerivedItem() );

                //cfg.MapTypes<Container, Container>()
                //   .MapMember( a => a.Data, b => b.Item );
            } );

            var target = mapper.Map( source );

            bool isResultOk = mapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void CollectionStruct()
        {
            var source = new Drawing()
            {
                Item = new DerivedItem()
                {
                    Points = new ObservableCollection<Point>() { new Point( 1, 1 ), new Point( 2, 2 ), new Point( 3, 3 ) }
                }
            };

            var mapper = new Mapper( cfg =>
            {
                //cfg.MapTypes<string, BaseItem>( a => (BaseItem)new DerivedItem() );

                //cfg.MapTypes<Container, Container>()
                //   .MapMember( a => a.Data, b => b.Item );
            } );

            var target = mapper.Map( source );

            bool isResultOk = mapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }

    [TestClass]
    public class RealWorldRequests
    {
        private class AInitialized
        {
            public int P1 { get; set; }
            public string P2 { get; set; }
            public List<string> P3 { get; set; }
            public List<int> P4 { get; set; } = new List<int>();
        }

        [TestMethod]
        public void CloneIgnoreSomePropertiesAndSetNull()
        {
            var source = new AInitialized()
            {
                P1 = 3,
                P2 = "hello",
                P3 = new List<string>() { "a", "b", "c" },
                P4 = new List<int>() { 1, 2, 3 }
            };

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<AInitialized, AInitialized>()
                    .MapMember( s => s.P4, t => t.P4, s => null )
                    .IgnoreSourceMember( s => s.P2 );
            } );

            var target = mapper.Map( source );

            Assert.IsTrue( target.P1 == source.P1 );
            Assert.IsTrue( target.P2 == null );
            Assert.IsTrue( target.P3.SequenceEqual( source.P3 ) );
            Assert.IsTrue( target.P4 == null );
        }

        [TestMethod]
        public void CloneIgnoreSomePropertiesAndSetNullIgnoreConventionResolvedMappings()
        {
            var source = new AInitialized()
            {
                P1 = 3,
                P2 = "hello",
                P3 = new List<string>() { "a", "b", "c" },
                P4 = new List<int>() { 1, 2, 3 }
            };

            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<AInitialized, AInitialized>( cfgType => cfgType.IgnoreMemberMappingResolvedByConvention = true )
                    .MapMember( s => s.P1, t => t.P1 )
                    .MapMember( s => s.P3, t => t.P3 )
                    .MapMember( s => s.P4, t => t.P4, s => null );
            } );

            var target = mapper.Map( source );

            Assert.IsTrue( target.P1 == source.P1 );
            Assert.IsTrue( target.P2 == null );
            Assert.IsTrue( target.P3.SequenceEqual( source.P3 ) );
            Assert.IsTrue( target.P4 == null );
        }
    }
}

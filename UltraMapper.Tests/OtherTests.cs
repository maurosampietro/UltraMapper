using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

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
}

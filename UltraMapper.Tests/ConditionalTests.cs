using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UltraMapper.Tests
{
    public class DoMap
    {
        private DoChildMap _nested;
        private List<DoChildMap> _children;
        private int _count;
        private int _collectionCount;

        public int NestedGetCount => _count;

        public bool UseNested { get; set; }

        public DoChildMap Nested
        {
            get
            {
                _count++;
                return _nested;
            }
            set => _nested = value;
        }

        public int NestedCollectionGetCount => _collectionCount;

        public bool UseNestedCollection { get; set; }

        public List<DoChildMap> NestedCollection
        {
            get
            {
                _collectionCount++;
                return _children;
            }
            set => _children = value;
        }
    }

    public class DoChildMap
    {
        public string MyName { get; set; }
    }

    [TestClass]
    public class ConditionalTests
    {
        [TestMethod]
        public void ConditionalMapSkippedSourceProp()
        {
            var source = new DoMap()
            {
                UseNested = false,
                Nested = new DoChildMap()
                {
                    MyName = "bla"
                }
            };
            var countBefore = source.NestedGetCount;
            var target = ConfiguredMapper.Map<DoMap, DoMap>( source );
            var countAfter = source.NestedGetCount;
            Assert.IsNotNull( source.Nested, "source" );
            Assert.IsNull( target.Nested, "target" );
            Assert.AreEqual( countBefore, countAfter );
        }

        [TestMethod]
        public void ConditionalMapNotSkippedSourceProp()
        {
            var source = new DoMap()
            {
                UseNested = true,
                Nested = new DoChildMap()
                {
                    MyName = "bla"
                }
            };
            var countBefore = source.NestedGetCount;
            var target = ConfiguredMapper.Map<DoMap, DoMap>( source );
            var countAfter = source.NestedGetCount;
            Assert.IsNotNull( target.Nested, "target" );
            Assert.IsNotNull( source.Nested, "source" );
            Assert.AreEqual( countBefore + 1, countAfter );
        }

        [TestMethod]
        public void ConditionalCollectionMapSkippedSourceProp()
        {
            var source = new DoMap()
            {
                UseNested = false,
                Nested = new DoChildMap()
                {
                    MyName = "bla"
                },
                UseNestedCollection = false,
                NestedCollection = new List<DoChildMap> {
                    new DoChildMap() { MyName="kwuk1"} ,
                    new DoChildMap() { MyName = "kwuk2" }
                }
            };
            var countBefore = source.NestedCollectionGetCount;
            var target = ConfiguredMapper.Map<DoMap, DoMap>( source );
            var countAfter = source.NestedCollectionGetCount;
            Assert.AreEqual( source.NestedCollection.Count, 2 );
            Assert.AreEqual( target.NestedCollection.Count, 0 );
            Assert.AreEqual( countBefore, countAfter );
        }

        [TestMethod]
        public void ConditionalCollectionMapNotSkippedSourceProp()
        {
            var source = new DoMap()
            {
                UseNested = false,
                Nested = new DoChildMap()
                {
                    MyName = "bla"
                },
                UseNestedCollection = true,
                NestedCollection = new List<DoChildMap> {
                    new DoChildMap() { MyName="kwuk1"} ,
                    new DoChildMap() { MyName = "kwuk2" }
                }
            };
            var countBefore = source.NestedCollectionGetCount;
            var target = ConfiguredMapper.Map<DoMap, DoMap>( source );
            var countAfter = source.NestedCollectionGetCount;
            Assert.AreEqual( source.NestedCollection.Count, 2 );
            Assert.AreEqual( target.NestedCollection.Count, 2 );
            Assert.AreEqual( countBefore + 1, countAfter );
        }

        private Mapper ConfiguredMapper =>
             new Mapper( cfg =>
            {
                cfg.MapTypes<DoMap, DoMap>()
                   .MapConditionalMember(
                        a => a.UseNested,
                        () => null,
                        a => a.Nested,
                        t => t.Nested
                   )
                   .MapConditionalMember(
                        a => a.UseNestedCollection,
                        () => new List<DoChildMap>(),
                        a => a.NestedCollection,
                        t => t.NestedCollection
                   );
            } );
    }
}
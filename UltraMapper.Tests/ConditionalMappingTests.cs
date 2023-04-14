using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ConditionalMappingTests
    {
        [TestMethod]
        public void MapConditionalShouldNotMap()
        {
            var source = new SourceTestClassWithConditional
            {
                ClassProperty = new SourceChildClass { A = 35645 },
                StructProperty = 45,
                NullableStructProperty = 3245,
                CollectionProperty = new List<string> { "hello", "there" }
            };

            var target = ConfiguredMapping.Map<TargetClassWithConditional>( source );
            Assert.AreEqual( 0, source.ClassGetCount );
            Assert.AreEqual( 0, source.StructGetCount );
            Assert.AreEqual( 0, source.NullableGetCount );
            Assert.AreEqual( 0, source.CollectionGetCount );

            Assert.IsNull( target.ClassProp );
            Assert.AreEqual( -1, target.Structo );
            Assert.IsNull( target.Nullable );
            Assert.AreEqual( 0, target.Collection.Count );
        }

        [TestMethod]
        public void MapConditionalShouldMap()
        {
            var source = new SourceTestClassWithConditional
            {
                ShouldMap = true,
                ClassProperty = new SourceChildClass { A = 35645 },
                StructProperty = 45,
                NullableStructProperty = 3245,
                CollectionProperty = new List<string> { "hello", "there" }
            };

            var target = ConfiguredMapping.Map<TargetClassWithConditional>( source );
            Assert.AreEqual( 1, source.ClassGetCount );
            Assert.AreEqual( 1, source.StructGetCount );
            Assert.AreEqual( 1, source.NullableGetCount );
            Assert.IsTrue( source.CollectionGetCount > 0 );

            Assert.AreEqual( 35645, target.ClassProp.A );
            Assert.AreEqual( 45, target.Structo );
            Assert.AreEqual( 3245, target.Nullable );
            Assert.AreEqual( 2, target.Collection.Count );
            Assert.AreEqual( "hello", target.Collection.First() );
            Assert.AreEqual( "there", target.Collection.Last() );
        }


        [TestMethod]
        public void MapConditionalSelfShouldNotMap()
        {
            var source = new SourceTestClassWithConditional
            {
                ClassProperty = new SourceChildClass { A = 35645 },
                StructProperty = 45,
                NullableStructProperty = 3245,
                CollectionProperty = new List<string> { "hello", "there" }
            };

            var target = ConfiguredMapping.Map<SourceTestClassWithConditional>( source );
            Assert.AreEqual( 0, source.ClassGetCount );
            Assert.AreEqual( 0, source.StructGetCount );
            Assert.AreEqual( 0, source.NullableGetCount );
            Assert.AreEqual( 0, source.CollectionGetCount );

            Assert.IsNull( target.ClassProperty );
            Assert.AreEqual( -1, target.StructProperty );
            Assert.IsNull( target.NullableStructProperty );
            Assert.AreEqual( 0, target.CollectionProperty.Count );
        }

        [TestMethod]
        public void MapConditionalSelfShouldMap()
        {
            var source = new SourceTestClassWithConditional
            {
                ShouldMap = true,
                ClassProperty = new SourceChildClass { A = 35645 },
                StructProperty = 45,
                NullableStructProperty = 3245,
                CollectionProperty = new List<string> { "hello", "there" }
            };

            var target = ConfiguredMapping.Map<SourceTestClassWithConditional>( source );
            Assert.AreEqual( 1, source.ClassGetCount );
            Assert.AreEqual( 1, source.StructGetCount );
            Assert.AreEqual( 1, source.NullableGetCount );
            Assert.IsTrue( source.CollectionGetCount > 0 );

            Assert.AreEqual( 35645, target.ClassProperty.A );
            Assert.AreEqual( 45, target.StructProperty );
            Assert.AreEqual( 3245, target.NullableStructProperty );
            Assert.AreEqual( 2, target.CollectionProperty.Count );
            Assert.AreEqual( "hello", target.CollectionProperty.First() );
            Assert.AreEqual( "there", target.CollectionProperty.Last() );
        }

        private Mapper ConfiguredMapping =>

               new Mapper( cfg =>
               {
                   cfg.MapTypes<SourceChildClass, SourceChildClass>();
                   cfg.MapTypes<SourceChildClass, TargetChildClass>();

                   cfg.MapTypes<SourceTestClassWithConditional, SourceTestClassWithConditional>()
                     .MapConditionalMember( x => x.ShouldMap, () => null, x => x.ClassProperty, y => y.ClassProperty )
                     .MapConditionalMember( x => x.ShouldMap, () => -1, x => x.StructProperty, y => y.StructProperty )
                     .MapConditionalMember( x => x.ShouldMap, () => null, x => x.NullableStructProperty, y => y.NullableStructProperty )
                     .MapConditionalMember( x => x.ShouldMap, () => new List<string>(), x => x.CollectionProperty, y => y.CollectionProperty );

                   cfg.MapTypes<SourceTestClassWithConditional, TargetClassWithConditional>()
                     .MapConditionalMember( x => x.ShouldMap, () => null, x => x.ClassProperty, y => y.ClassProp )
                     .MapConditionalMember( x => x.ShouldMap, () => -1, x => x.StructProperty, y => y.Structo )
                     .MapConditionalMember( x => x.ShouldMap, () => null, x => x.NullableStructProperty, y => y.Nullable )
                     .MapConditionalMember( x => x.ShouldMap, () => new List<string>(), x => x.CollectionProperty, y => y.Collection );
               } );



        public class SourceTestClassWithConditional
        {
            private SourceChildClass _child;
            private int _structChild;
            private int? _nullableChild;
            private List<string> _collectionChild;

            private int _getCountClass;
            private int _getCountStruct;
            private int _getCountNullable;
            private int _getCountCollection;
            public bool ShouldMap { get; set; }

            public int ClassGetCount => _getCountClass;
            public int StructGetCount => _getCountStruct;
            public int NullableGetCount => _getCountNullable;
            public int CollectionGetCount => _getCountCollection;

            public SourceChildClass ClassProperty
            {
                get { _getCountClass++; return _child; }
                set { _child = value; }
            }

            public int StructProperty
            {
                get { _getCountStruct++; return _structChild; }
                set { _structChild = value; }
            }


            public int? NullableStructProperty
            {
                get { _getCountNullable++; return _nullableChild; }
                set { _nullableChild = value; }
            }

            public List<string> CollectionProperty
            {
                get { _getCountCollection++; return _collectionChild; }
                set { _collectionChild = value; }
            }
        }

        public class SourceChildClass
        {
            public int A { get; set; }
        }

        public class TargetChildClass
        {
            public int A { get; set; }
        }

        public class TargetClassWithConditional
        {
            public TargetChildClass ClassProp { get; set; }
            public int Structo { get; set; }
            public int? Nullable { get; set; }
            public List<String> Collection { get; set; }
        }





    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InheritTypeMappingTests
    {
        public class TestClass
        {
            public bool Boolean { get; set; }
            public string String { get; set; }
            public List<string> Strings { get; set; } = new List<string>();
            public List<bool> Booleans { get; set; } = new List<bool>();
        }

        public class SubTestClass : TestClass
        {
            public int Integer { get; set; }
        }

        public class Container
        {
            public TestClass TestClass { get; set; }
        }

        [TestMethod]
        public void InheritMapping()
        {
            var source = new TestClass();
            source.Strings.Clear();
            source.Booleans.Clear();

            source.Booleans.Add( true );
            source.Booleans.Add( false );

            var target = new TestClass();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<bool, string>( b => b ? "1" : "0" );

                cfg.MapTypes<TestClass, TestClass>()
                    .MapMember( a => a.Boolean, y => y.String )
                    .MapMember( a => a.Booleans, y => y.Strings );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Boolean ? target.String == "1"
                : target.String == "0" );

            Assert.IsTrue( target.Strings.Contains( "1" ) &&
                target.String.Contains( "0" ) );
        }

        /// <summary>
        /// Should not map to the declared type,
        /// but to the runtimetype which could be 
        /// a type inherited/derived from the declared type.
        /// This already works if the declared type is abstract
        /// </summary>
        [TestMethod]
        public void ShouldMapToRuntimeUsedType()
        {
            var source = new Container()
            {
                TestClass = new SubTestClass()
                {
                    Boolean = true,
                    String = "ciao",
                    //Integer = 11
                }
            };

            var target = new Container();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            Assert.IsTrue( target.TestClass.GetType() == source.TestClass.GetType() );

            var isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void CollectionUpdateInheritance()
        {
            var source = new ReadOnlyCollection<TestClass>( new TestClass[]
            {
                new TestClass() { String = "A" },
                new TestClass() { String = "B" } }
            );

            var target = new ObservableCollection<TestClass>();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<IEnumerable<TestClass>, IEnumerable<TestClass>, TestClass, TestClass>( ( s, t ) => s.String == t.String );
            } );

            ultraMapper.Map( source, target );

            var userDefinedTypePair = new Internals.TypePair( typeof( IEnumerable<TestClass> ), typeof( IEnumerable<TestClass> ) );
            var userDefinedMap = ultraMapper.Config[ userDefinedTypePair ];

            var inheritedTypePair = new Internals.TypePair( typeof( ReadOnlyCollection<TestClass> ), typeof( ObservableCollection<TestClass> ) );
            var conventionDefinedMap = ultraMapper.Config[ inheritedTypePair ];

            var conventionDefinedMapOptionCrawler = new TypeMappingOptionsInheritanceTraversal( conventionDefinedMap );

            Assert.IsTrue( userDefinedMap.CollectionBehavior == conventionDefinedMapOptionCrawler.CollectionBehavior );
            Assert.IsTrue( userDefinedMap.CollectionItemEqualityComparer == conventionDefinedMapOptionCrawler.CollectionItemEqualityComparer );
            Assert.IsTrue( userDefinedMap.ReferenceBehavior == conventionDefinedMapOptionCrawler.ReferenceBehavior );

            var isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

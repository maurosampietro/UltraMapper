using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InheritTypeMappingTests
    {
        private class ConfigOverrideTestType
        {
            public List<int> List1 { get; set; }
            public List<int> List2 { get; set; }
            public List<int> List3 { get; set; }
        }

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

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<TestClass, TestClass>( () => new SubTestClass() );
                cfg.MapTypes<Container, Container>().MapMember
                (
                    s => s.TestClass,
                    t => t.TestClass,
                    memberMappingConfig: cfg2 =>
                    {
                        cfg2.SetCustomTargetConstructor( () => new SubTestClass() );
                    }
                );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.TestClass.GetType() == target.TestClass.GetType() );

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

            var userDefinedMap = ultraMapper.Config[ typeof( IEnumerable<TestClass> ), typeof( IEnumerable<TestClass> ) ];
            var conventionDefinedMap = ultraMapper.Config[ typeof( ReadOnlyCollection<TestClass> ), typeof( ObservableCollection<TestClass> ) ];
            //var conventionDefinedMapOptionCrawler = new TypeMappingOptionsInheritanceTraversal( conventionDefinedMap );

            //Assert.IsTrue( userDefinedMap.CollectionBehavior == conventionDefinedMapOptionCrawler.CollectionBehavior );
            //Assert.IsTrue( userDefinedMap.CollectionItemEqualityComparer == conventionDefinedMapOptionCrawler.CollectionItemEqualityComparer );
            //Assert.IsTrue( userDefinedMap.ReferenceBehavior == conventionDefinedMapOptionCrawler.ReferenceBehavior );

            var isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        //[Ignore]
        public void ConfigurationOptionOverride()
        {
            //In this test the collection update adds elements to the target.
            //This works if the capacity of the target list is updated BEFORE adding elements.

            var source = new ConfigOverrideTestType()
            {
                List1 = Enumerable.Range( 1, 10 ).ToList(),
                List2 = Enumerable.Range( 20, 10 ).ToList(),
                List3 = Enumerable.Range( 30, 10 ).ToList()
            };

            var target = new ConfigOverrideTestType()
            {
                List1 = Enumerable.Range( 40, 10 ).ToList(),
                List2 = Enumerable.Range( 50, 10 ).ToList(),
                List3 = Enumerable.Range( 60, 10 ).ToList()
            };

            var targetPrimitiveListCount = target.List1.Count;

            var mapper = new Mapper( cfg =>
            {
                cfg.CollectionBehavior = CollectionBehaviors.UPDATE;
                cfg.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;

                cfg.MapTypes<ConfigOverrideTestType, ConfigOverrideTestType>( typeConfig =>
                {
                    typeConfig.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
                    typeConfig.CollectionBehavior = CollectionBehaviors.MERGE;
                } )
                .MapMember( s => s.List1, t => t.List1, memberConfig =>
                {
                    memberConfig.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                    memberConfig.CollectionBehavior = CollectionBehaviors.MERGE;
                } )
                .MapMember( s => s.List2, t => t.List2, memberConfig =>
                {
                    memberConfig.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
                    memberConfig.CollectionBehavior = CollectionBehaviors.RESET;
                } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( target.List1.SequenceEqual( Enumerable.Range( 40, 10 ).Concat( Enumerable.Range( 1, 10 ) ) ) );
            Assert.IsTrue( source.List2.SequenceEqual( target.List2 ) );
            Assert.IsTrue( source.List3.SequenceEqual( target.List3 ) );
        }
    }
}

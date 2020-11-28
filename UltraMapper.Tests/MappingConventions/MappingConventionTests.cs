using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class MappingConventionTests
    {
        private class TestType
        {
            public List<int> List1 { get; set; }
            public List<int> List2 { get; set; }
        }

        private class GetMethodConventions
        {
            public List<int> GetList1()
            {
                return new List<int> { 0, 1, 2 };
            }

            public List<int> Get_List2()
            {
                return new List<int> { 3, 4, 5 };
            }
        }

        private class SetMethodConventions
        {
            private List<int> _list1;
            private List<int> _list2;

            public void SetList1( List<int> value ) { _list1 = value; }
            public void Set_List2( List<int> value ) { _list2 = value; }

            public List<int> GetList1() { return _list1; }
            public List<int> GetList2() { return _list2; }
        }

        [TestMethod]
        [Ignore]
        public void EnableConventionsGlobally()
        {

        }

        [TestMethod]
        [Ignore]
        public void EnableConventionsGloballyButDisableOnSpecificType()
        {

        }

        [TestMethod]
        [Ignore]
        public void DisableConventionsGlobally()
        {

        }

        [TestMethod]
        [Ignore]
        public void DisableConventionsGloballyButEnableOnSpecificType()
        {

        }

        [TestMethod]
        public void ConfigurationOptionOverride()
        {
            //In this test the collection update adds elements to the target.
            //This works if the capacity of the target list is updated BEFORE adding elements.

            var source = new TestType()
            {
                List1 = Enumerable.Range( 1, 10 ).ToList(),
                List2 = Enumerable.Range( 20, 10 ).ToList()
            };

            var target = new TestType()
            {
                List1 = Enumerable.Range( 30, 10 ).ToList(),
                List2 = Enumerable.Range( 40, 10 ).ToList()
            };

            var targetPrimitiveListCount = target.List1.Count;

            var mapper = new Mapper( cfg =>
            {
                cfg.CollectionBehavior = CollectionBehaviors.UPDATE;
                cfg.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;

                cfg.MapTypes<TestType, TestType>( typeConfig =>
                {
                    typeConfig.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
                    typeConfig.CollectionBehavior = CollectionBehaviors.RESET;
                } )
                .MapMember( s => s.List1, t => t.List1, memberConfig =>
                {
                    memberConfig.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                    memberConfig.CollectionBehavior = CollectionBehaviors.MERGE;
                } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( target.List1.SequenceEqual( Enumerable.Range( 30, 10 ).Concat( Enumerable.Range( 1, 10 ) ) ) );
            Assert.IsTrue( source.List2.SequenceEqual( target.List2 ) );
        }

        [TestMethod]
        public void MethodMatching()
        {
            var source = new GetMethodConventions();
            var target = new SetMethodConventions();

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.SourceMemberProvider.IgnoreMethods = false;
                    convention.TargetMemberProvider.IgnoreMethods = false;

                    convention.MatchingRules.GetOrAdd<MethodMatching>();
                } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( source.GetList1().SequenceEqual( target.GetList1() ) );
            Assert.IsTrue( source.Get_List2().SequenceEqual( target.GetList2() ) );
        }

        private class A
        {
            public double Double { get; set; }
        }

        private class B
        {
            public float Double { get; set; }
        }

        [TestMethod]
        public void ExactNameAndExplicitConversionTypeMatching()
        {
            var source = new A() { Double = 11 };

            var mapper = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = false );
                } );
            } );

            var result = mapper.Map<B>( source );
            Assert.IsTrue( result.Double == 0 );

            var mapper2 = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = true );
                } );
            } );

            result = mapper2.Map<B>( source );
            Assert.IsTrue( result.Double == 11 );
        }

        private class C
        {
            public float Double { get; set; }
        }

        private class D
        {
            public double Double { get; set; }
        }

        [TestMethod]
        public void ExactNameAndConversionTypeMatching()
        {
            var source = new C() { Double = 11 };

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowImplicitConversions = false );
                } );
            } );

            var result = mapper.Map<D>( source );
            Assert.IsTrue( result.Double == 0 );

            var mapper2 = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = true );
                } );
            } );

            result = mapper2.Map<D>( source );
            Assert.IsTrue( result.Double == 11 );
        }

        private class E
        {
            public double? Double { get; set; }
        }

        private class F
        {
            public double Double { get; set; }
        }
    }
}

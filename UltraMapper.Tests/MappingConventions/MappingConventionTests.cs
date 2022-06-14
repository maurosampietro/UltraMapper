using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Conventions;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class MappingConventionTests
    {
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

                    convention.MatchingRules.GetOrAdd<MethodNameMatching>();
                    convention.MatchingRules.GetOrAdd<MethodTypeMatching>();
                    convention.MatchingRules.GetOrAdd<TypeMatchingRule>();
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
                var matching = new TypeSet<IMatchingRule>();

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
            public float Number { get; set; }
        }

        private class D
        {
            public double Number { get; set; }
        }

        [TestMethod]
        public void ExactNameAndConversionTypeMatching()
        {
            var source = new C() { Number = 11 };

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowImplicitConversions = false );
                } );
            } );

            var result = mapper.Map<D>( source );
            Assert.IsTrue( result.Number == 0 );

            var mapper2 = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = true );
                } );
            } );

            result = mapper2.Map<D>( source );
            Assert.IsTrue( result.Number == 11 );
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class MatchingRuleTests
    {
        public class TestClass
        {
            public int Property { get; set; }
        }

        public class PrefixTestClass
        {
            public int DtoProperty { get; set; }
        }

        public class SuffixTestClass
        {
            public int PropertyDto { get; set; }
        }

        [TestMethod]
        public void PrefixMatching()
        {
            //Property -> DtoProperty
            var source2 = new TestClass() { Property = 11 };
            var mapper2 = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<PrefixMatching>();
                } );
            } );

            var result2 = mapper2.Map<PrefixTestClass>( source2 );
            Assert.IsTrue( source2.Property == result2.DtoProperty );

            //DtoProperty -> Property
            var source = new PrefixTestClass() { DtoProperty = 11 };
            var mapper = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<PrefixMatching>();
                } );
            } );

            var result = mapper.Map<TestClass>( source );
            Assert.IsTrue( source.DtoProperty == result.Property );
        }

        [TestMethod]
        public void SuffixMatching()
        {
            //Property -> DtoProperty
            var source2 = new TestClass() { Property = 11 };
            var mapper2 = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<SuffixMatching>();
                } );
            } );

            var result2 = mapper2.Map<SuffixTestClass>( source2 );
            Assert.IsTrue( source2.Property == result2.PropertyDto );

            //DtoProperty -> Property
            var source = new SuffixTestClass() { PropertyDto = 11 };
            var mapper = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<SuffixMatching>();
                } );
            } );

            var result = mapper.Map<TestClass>( source );
            Assert.IsTrue( source.PropertyDto == result.Property );
        }
    }
}

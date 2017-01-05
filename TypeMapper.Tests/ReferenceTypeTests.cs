using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace TypeMapper.Tests
{
    [TestClass]
    public class ReferenceTypeTests
    {
        private class OuterType
        {
            public InnerType NullInnerType { get; set; }
            public InnerType InnerType { get; set; }
            public string String { get; set; }

            public OuterType()
            {
                this.InnerType = new InnerType()
                {
                    A = "a",
                    B = "b"
                };
            }
        }

        private class OuterTypeDto
        {
            public InnerTypeDto NullInnerTypeDto { get; set; }
            public InnerTypeDto InnerTypeDto { get; set; }

            public OuterTypeDto()
            {
                this.NullInnerTypeDto = new InnerTypeDto();
                this.InnerTypeDto = new InnerTypeDto();
            }
        }

        private class InnerType
        {
            public string A { get; set; }
            public string B { get; set; }

            public OuterType C { get; set; }
        }

        private class InnerTypeDto
        {
            public string A { get; set; }
            public string B { get; set; }

            public OuterType C { get; set; }
        }

        [TestMethod]
        public void UseTargetInstanceIfNotNull()
        {
            var source = new OuterType() { String = "Test" };
            var target = new OuterType() { String = "fadadfsadsffsd" };

            string expectedValue = target.String;

            var typeMapper = new TypeMapper<CustomMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;

                cfg.GlobalConfiguration.MappingConvention.PropertyMatchingRules
                    //.GetOrAdd<TypeMatchingRule>( rule => rule.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( rule => rule.IgnoreCase = true )
                    .Respect( ( /*rule1,*/ rule2, rule3 ) => /*rule1 & */(rule2 | rule3) );

                cfg.MapTypes( source, target ).IgnoreSourceProperty( s => s.String );
            } );

            typeMapper.Map( source, target );
            Assert.IsTrue( target.String == expectedValue );
        }

        [TestMethod]
        public void CreateNewInstance()
        {
            var source = new OuterType() { String = "Test" };
            var target = new OuterType() { String = "fadadfsadsffsd" };

            string expectedValue = target.String;

            var typeMapper = new TypeMapper<CustomMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.ReferenceMappingStrategy = ReferenceMappingStrategies.CREATE_NEW_INSTANCE;

                cfg.GlobalConfiguration.MappingConvention.PropertyMatchingRules
                    //.GetOrAdd<TypeMatchingRule>( rule => rule.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( rule => rule.IgnoreCase = true )
                    .Respect( ( /*rule1,*/ rule2, rule3 ) => /*rule1 & */(rule2 | rule3) );

                cfg.MapTypes( source, target ).IgnoreSourceProperty( s => s.String );
            } );

            typeMapper.Map( source, target );
            Assert.IsTrue( target.String == null );
        }

        [TestMethod]
        public void ManualFlattening()
        {
            var source = new OuterType();
            var target = new InnerTypeDto();

            var typeMapper = new TypeMapper<CustomMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.MappingConvention.PropertyMatchingRules
                    //.GetOrAdd<TypeMatchingRule>( rule => rule.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( rule => rule.IgnoreCase = true )
                    .Respect( ( /*rule1,*/ rule2, rule3 ) => /*rule1 & */(rule2 | rule3) );

                cfg.MapTypes<OuterType, InnerTypeDto>()
                    .MapProperty( a => a.InnerType.A, b => b.A );
            } );

            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

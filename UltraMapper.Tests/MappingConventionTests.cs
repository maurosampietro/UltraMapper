using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [TestMethod]
        public void EnableConventionsGlobally()
        {

        }

        [TestMethod]
        public void EnableConventionsGloballyButDisableOnSpecificType()
        {

        }

        [TestMethod]
        public void DisableConventionsGlobally()
        {

        }

        [TestMethod]
        public void DisableConventionsGloballyButEnableOnSpecificType()
        {

        }

        [TestMethod]
        public void Configuration()
        {
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

            var mapper = new UltraMapper( cfg =>
            {
                cfg.CollectionMappingStrategy = CollectionMappingStrategies.UPDATE;
                cfg.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;

                cfg.MapTypes<TestType, TestType>( typeConfig =>
                {
                    typeConfig.ReferenceMappingStrategy = ReferenceMappingStrategies.CREATE_NEW_INSTANCE;
                    typeConfig.CollectionMappingStrategy = CollectionMappingStrategies.RESET;
                } )
                .MapMember( s => s.List1, t => t.List1, memberConfig =>
                {
                    memberConfig.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
                    memberConfig.CollectionMappingStrategy = CollectionMappingStrategies.MERGE;
                } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( target.List1.SequenceEqual( Enumerable.Range( 30, 10 ).Concat( Enumerable.Range( 1, 10 ) ) ) );
            Assert.IsTrue( source.List2.SequenceEqual( target.List2 ) );
        }
    }
}

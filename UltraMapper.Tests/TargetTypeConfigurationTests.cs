using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UltraMapper.Tests
{
    [TestClass]
    public class TargetTypeConfigurationTests
    {
        public class Target : TargetImplementsThis { }

        public interface TargetDontImplementThis { }

        public interface TargetImplementsThis { }

        public class UnrelatedClass { }

        public class DerivedFromTarget:Target { }

        [TestMethod]
        public void TargetAssignableFromImplementedInstance()
        {
            Assert.ThrowsException<ArgumentException>( () =>
            {
                var targetTypeConfig = new TargetTypeConfiguration();
                targetTypeConfig[ typeof( Target ) ] = typeof( TargetImplementsThis );
            } );
        }

        [TestMethod]
        public void TargetAssignableFromNonImplementedInstance()
        {
            Assert.ThrowsException<ArgumentException>( () =>
            {
                var targetTypeConfig = new TargetTypeConfiguration();
                targetTypeConfig[ typeof( Target ) ] = typeof( TargetDontImplementThis );
            } );
        }

        [TestMethod]
        public void TargetNotAssignableFromUnrelatedClass()
        {
            Assert.ThrowsException<ArgumentException>( () =>
            {
                var targetTypeConfig = new TargetTypeConfiguration();
                targetTypeConfig[ typeof( Target ) ] = typeof( UnrelatedClass );
            } );
        }

        [TestMethod]
        public void TargetAssignableFromClassDerivedFromTarget()
        {
            var targetTypeConfig = new TargetTypeConfiguration();
            targetTypeConfig[ typeof( Target ) ] = typeof( DerivedFromTarget );
        }

        [TestMethod]
        public void TargetAssignableFromClassesImplementingSameInterface()
        {
            var targetTypeConfig = new TargetTypeConfiguration();
            targetTypeConfig[ typeof( TargetImplementsThis ) ] = typeof( DerivedFromTarget );
            targetTypeConfig[ typeof( TargetImplementsThis ) ] = typeof( Target );
            targetTypeConfig[ typeof( IEnumerable<string> ) ] = typeof( List<string> );
            //targetTypeConfig[ typeof( IEnumerable<> ) ] = typeof( List<> );
        }
    }
}

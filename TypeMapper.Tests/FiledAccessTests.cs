using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;
using TypeMapper;
using TypeMapper.Mappers.TypeMappers;

namespace TypeMapper.Tests
{
    [TestClass]
    public class FieldAccess
    {
        private class ComplexType
        {
            public string FieldA;
        }

        [TestMethod]
        public void SimpleFieldAccess()
        {
            var source = new ComplexType() { FieldA = "test" };
            var target = new ComplexType() { FieldA = "overwrite this" };

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.FieldA == target.FieldA );
        }
    }
}

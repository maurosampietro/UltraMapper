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
    /// <summary>
    /// Test all the possible kinds of member selection combinations
    /// (fields, properties and method calls ).
    /// </summary>
    [TestClass]
    public class ValueAccessTests
    {
        private class TestType
        {
            public string FieldA;

            public string PropertyA
            {
                get { return FieldA; }
                set { FieldA = value; }
            }

            public string GetFieldA() { return FieldA; }
            public void SetFieldA( string value ) { FieldA = value; }
        }

        [TestMethod]
        public void PropertyToProperty()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapProperty( s => s.PropertyA, t => t.PropertyA )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void PropertyToField()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapProperty( s => s.PropertyA, t => t.FieldA )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void PropertyToSetterMethod()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapMethod( s => s.PropertyA, ( t, val ) => t.SetFieldA( val ) )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void FieldToField()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapProperty( s => s.FieldA, t => t.FieldA )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void FieldToProperty()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapProperty( s => s.FieldA, t => t.PropertyA )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void FieldToSetterMethod()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapMethod( s => s.FieldA, ( t, val ) => t.SetFieldA( val ) )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void GetterMethodToSetterMethod()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapMethod( s => s.GetFieldA(), ( t, val ) => t.SetFieldA( val ) )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void GetterMethodToProperty()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapProperty( s => s.GetFieldA(), t => t.PropertyA )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }

        [TestMethod]
        public void GetterMethodToField()
        {
            var source = new TestType() { PropertyA = "test" };
            var target = new TestType() { PropertyA = "overwrite this" };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<TestType, TestType>()
                    .MapProperty( s => s.GetFieldA(), t => t.FieldA )
            );
            typeMapper.Map( source, target );

            Assert.IsTrue( !Object.ReferenceEquals( source, target ) );
            Assert.IsTrue( source.PropertyA == target.PropertyA );
        }
    }
}

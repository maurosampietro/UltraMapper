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
    public class DirectMappings
    {
        private class ComplexType
        {
            public int GetPropertyA() { return this.PropertyA; }
            public int PropertyA { get; set; }

            public override bool Equals( object obj )
            {
                return (obj as ComplexType)?.PropertyA == this.PropertyA;
            }

            public override int GetHashCode()
            {
                return PropertyA;
            }
        }

        [TestMethod]
        public void PrimitiveToSamePrimitive()
        {
            int source = 10;
            int target = 13;

            Assert.IsTrue( source != target );

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, ref target );

            Assert.IsTrue( source == target );
        }

        [TestMethod]
        public void PrimitiveToDifferentPrimitive()
        {
            int source = 10;
            double target = 13;

            Assert.IsTrue( source != target );

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, ref target );

            Assert.IsTrue( source == target );
        }

        [TestMethod]
        public void ListToListSameElementSimpleType()
        {
            List<int> source = Enumerable.Range( 0, 10 ).ToList();
            List<int> target = Enumerable.Range( 10, 10 ).ToList();

            Assert.IsTrue( !source.SequenceEqual( target ) );

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual( target ) );
        }

        [TestMethod]
        public void ListToListSameElementComplexType()
        {
            var source = new List<ComplexType>()
            {
                new ComplexType() { PropertyA = 1 },
                new ComplexType() { PropertyA = 2 }
            };

            var target = new List<ComplexType>();

            Assert.IsTrue( !source.SequenceEqual( target ) );

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual( target ) );
        }

        [TestMethod]
        public void ListToListDifferentElementType()
        {
            List<int> source = Enumerable.Range( 0, 10 ).ToList();
            List<double> target = new List<double>() { 1, 2, 3 };

            Assert.IsTrue( !source.SequenceEqual(
                target.Select( item => (int)item ) ) );

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            Assert.IsTrue( source.SequenceEqual(
                target.Select( item => (int)item ) ) );
        }

        [TestMethod]
        public void FromPrimitiveCollectionToComplexCollection()
        {
            //var source = new List<int>() { 11, 13, 17 };

            //var target = new List<ComplexType>()
            //{
            //    new ComplexType() { PropertyA = 1 },
            //    new ComplexType() { PropertyA = 2 }
            //};

            //var typeMapper = new TypeMapper
            //(
            //    cfg => cfg.MapTypes<int, ComplexType>()
            //    //loop infinito causato dalla selezione di se stessi 'a =>a'
            //        .MapProperty( a => a, c => c.PropertyA )
            //);

            //typeMapper.Map( source, target );
            //typeMapper.VerifyMapperResult( source, target );
            throw new NotImplementedException();
        }

        [TestMethod]
        public void FromComplexCollectionToPrimitiveCollection()
        {
            var source = new List<ComplexType>()
            {
                new ComplexType() { PropertyA = 1 },
                new ComplexType() { PropertyA = 2 }
            };

            var target = new List<int>() { 11, 13, 17 };

            var typeMapper = new TypeMapper
            (
                cfg => cfg.MapTypes<ComplexType, ComplexType>()
                    .MapProperty( a => a.PropertyA, c => c.PropertyA )
            );

            typeMapper.Map( source, target );
            typeMapper.VerifyMapperResult( source, target );
        }
    }
}

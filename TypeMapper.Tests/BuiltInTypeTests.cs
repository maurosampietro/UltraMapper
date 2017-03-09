using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.MappingConventions;

namespace TypeMapper.Tests
{
    [TestClass]
    public class BuiltInTypeTests
    {
        private class PrimitiveTypes
        {
            public bool Boolean { get; set; } = true;
            public byte Byte { get; set; } = 0x1;
            public char Char { get; set; } = (char)2;
            public decimal Decimal { get; set; } = 3;
            public double Double { get; set; } = 4.0;
            public short Int16 { get; set; } = 5;
            public int Int32 { get; set; } = 6;
            public long Int64 { get; set; } = 7;
            public sbyte SByte { get; set; } = 0x8;
            public float Single { get; set; } = 9.0f;
            public ushort UInt16 { get; set; } = 10;
            public uint UInt32 { get; set; } = 11;
            public ulong UInt64 { get; set; } = 12;
        }

        //Inheritance is tested too
        private class BuiltInTypes : PrimitiveTypes
        {
            public object Object { get; set; } = 13;
            public string String { get; set; } = "14";
        }

        private class NullablePrimitiveTypes
        {
            public bool? Boolean { get; set; } = null;
            public byte? Byte { get; set; } = null;
            public sbyte? SByte { get; set; } = null;
            public char? Char { get; set; } = null;
            public decimal? Decimal { get; set; } = null;
            public double? Double { get; set; } = null;
            public short? Int16 { get; set; } = null;
            public int? Int32 { get; set; } = null;
            public long? Int64 { get; set; } = null;
            public float? Single { get; set; } = null;
            public ushort? UInt16 { get; set; } = null;
            public uint? UInt32 { get; set; } = null;
            public ulong? UInt64 { get; set; } = null;
        }

        private class NullableBuiltInTypes : NullablePrimitiveTypes
        {
            public object Object { get; set; }
            public string String { get; set; }
        }

        [TestMethod]
        public void BuiltInToBuiltIn()
        {
            var source = new BuiltInTypes();
            var target = new BuiltInTypes();

            var typeMapper = new TypeMapper( cfg =>
            {
                cfg.MapTypes<BuiltInTypes, BuiltInTypes>()
                    //map with custom converter
                    .MapMember( a => a.Single, d => d.String, single => single.ToString() )

                    //map same source property to many different targets
                    .MapMember( a => a.Char, d => d.Single )
                    .MapMember( a => a.Char, d => d.Int32 )

                    //same sourceproperty/destinationProperty: second mapping overrides and adds the converter 
                    .MapMember( a => a.String, d => d.Single )
                    .MapMember( a => a.String, d => d.Single, @string => Single.Parse( @string ) )

                    //same sourceproperty/destinationProperty: second mapping overrides and removes (set to null) the converter
                    .MapMember( a => a.Single, y => y.Double, a => a + 254 )
                    .MapMember( a => a.Single, y => y.Double );
            } );

            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void BuiltInToNullable()
        {
            var source = new BuiltInTypes();
            var target = new NullableBuiltInTypes();

            var typeMapper = new TypeMapper
            (
                cfg =>
                {
                    cfg.MapTypes<BuiltInTypes, NullablePrimitiveTypes>()
                       .MapMember( s => s.Int32, s => s.Char );
                }
            );

            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void NullableToNullable()
        {
            var source = new NullableBuiltInTypes();
            var target = new NullableBuiltInTypes();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void NullableToBuiltIn()
        {
            var source = new NullableBuiltInTypes()
            {
                Boolean = true,
                Byte = 0x1,
                Char = '2',
                Decimal = 3,
                Double = 4.0,
                Int16 = 5,
                Int32 = 6,
                Int64 = 7,
                Object = 8,
                SByte = 0x9,
                Single = 10,
                String = "11",
                UInt16 = 12,
                UInt32 = 13,
                UInt64 = 14
            };

            var target = new BuiltInTypes()
            {
                Boolean = false,
                Byte = 15,
                Char = (char)16,
                Decimal = 17,
                Double = 18,
                Int16 = 19,
                Int32 = 20,
                Int64 = 21,
                Object = 22,
                SByte = 0x23,
                Single = 24,
                String = "25",
                UInt16 = 26,
                UInt32 = 27,
                UInt64 = 28
            };

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void NullNullablesToDefaultPrimitives()
        {
            var source = new NullableBuiltInTypes();
            var target = new BuiltInTypes()
            {
                Boolean = true,
                Byte = 0x1,
                Char = (char)2,
                Decimal = 3,
                Double = 4.0,
                Int16 = 5,
                Int32 = 6,
                Int64 = 7,
                Object = 8,
                SByte = 0x9,
                Single = 10f,
                String = "11",
                UInt16 = 12,
                UInt32 = 13,
                UInt64 = 14,
            };

            //each property must be set to null
            Assert.IsTrue( source.GetType().GetProperties()
                .All( p => p.GetValue( source ) == null ) );

            //each property must be set to a non-default value
            Assert.IsTrue( target.GetType().GetProperties()
                .All( p =>
                {
                    object defaultValue = p.PropertyType.GetDefaultValueViaActivator();
                    return !p.GetValue( target ).Equals( defaultValue );
                } ) );

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            var isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        //pensare meglio a testare i tipi compatibili implicitamente
        //esplicitamente e tramite conversione
        //[TestMethod]
        //public void NullableToIncompatibleBuiltInTypes()
        //{
        //    var source = new NullableBuiltInTypes();
        //    var target = new BuiltInTypes()
        //    {
        //        Boolean = true,
        //        Byte = 0x1,
        //        Char = 'a',
        //        Decimal = 3,
        //        Double = 4.0,
        //        Int16 = 10,
        //        Int32 = 6,
        //        Int64 = 8,
        //        Object = 15,
        //        SByte = 0x2,
        //        Single = 5.0f,
        //        String = "12",
        //        UInt16 = 11,
        //        UInt32 = 7,
        //        UInt64 = 9,
        //    };

        //    //each property must be set to null
        //    Assert.IsTrue( source.GetType().GetProperties()
        //        .All( p => p.GetValue( source ) == null ) );

        //    //each property must be set to a non-default value
        //    Assert.IsTrue( target.GetType().GetProperties()
        //        .All( p =>
        //        {
        //            object defaultValue = p.PropertyType.GetDefaultValue();
        //            return !p.GetValue( target ).Equals( defaultValue );
        //        } ) );

        //    var typeMapper = new TypeMapper
        //    (
        //        cfg =>
        //        {
        //            cfg.GlobalConfiguration.IgnoreConventions = true;

        //            cfg.MapTypes<NullableBuiltInTypes, BuiltInTypes>()
        //                .MapProperty( s => s.Int32, s => s.Char );
        //        }
        //    );

        //    typeMapper.Map( source, target );
        //    typeMapper.VerifyMapperResult( source, target );
        //}
    }
}

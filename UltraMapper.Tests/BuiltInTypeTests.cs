using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UltraMapper.Internals;

namespace UltraMapper.Tests
{
    [TestClass]
    public class BuiltInTypeTests
    {
        private class PrimitiveTypes
        {
            public bool Boolean { get; set; }
            public byte Byte { get; set; }
            public char Char { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public short Int16 { get; set; }
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public sbyte SByte { get; set; }
            public float Single { get; set; }
            public ushort UInt16 { get; set; }
            public uint UInt32 { get; set; }
            public ulong UInt64 { get; set; }
        }

        //Inheritance is tested too
        private class BuiltInTypes : PrimitiveTypes
        {
            public string String { get; set; }
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
            public string String { get; set; }
        }

        [TestMethod]
        public void BuiltInToBuiltIn()
        {
            var source = new BuiltInTypes()
            {
                Boolean = true,
                Byte = 0x1,
                Char = (char)2,
                Decimal = 3,
                Double = 4.0,
                Int16 = 5,
                Int32 = 6,
                Int64 = 7,
                SByte = 0x8,
                Single = 9.0f,
                UInt16 = 10,
                UInt32 = 11,
                UInt64 = 12,

                String = "13"
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<BuiltInTypes, BuiltInTypes>()
                    //map with custom converter
                    .MapMember( a => a.Single, d => d.String, single => single.ToString() )

                    //map same source property to many different targets
                    .MapMember( a => a.Char, d => d.Int32 )

                    //same source and destination members: last mapping overrides and adds the converter 
                    .MapMember( a => a.Char, d => d.Single )
                    .MapMember( a => 123, d => d.Single )
                    .MapMember( a => a.String, d => d.Single )
                    .MapMember( a => a.String, d => d.Single, @string => Single.Parse( @string ) )


                    //same sourceproperty/destinationProperty: second mapping overrides and removes (set to null) the converter
                    .MapMember( a => a.Single, y => y.Double, a => a + 254 )
                    .MapMember( a => a.Single, y => y.Double );
            } );

            var target = ultraMapper.Map( source );

            Assert.IsTrue( target.Boolean );
            Assert.IsTrue( target.Byte == 0x1 );
            Assert.IsTrue( target.Char == (char)2 );
            Assert.IsTrue( target.Decimal == 3 );
            Assert.IsTrue( target.Double == 9.0 );
            Assert.IsTrue( target.Int16 == 5 );
            Assert.IsTrue( target.Int32 == 2 );
            Assert.IsTrue( target.Int64 == 7 );
            Assert.IsTrue( target.SByte == 0x8 );
            Assert.IsTrue( target.Single == 13 );
            Assert.IsTrue( target.UInt16 == 10 );
            Assert.IsTrue( target.UInt32 == 11 );
            Assert.IsTrue( target.UInt64 == 12 );
        }

        [TestMethod]
        public void BuiltInToBuiltInConditional()
        {
            var source = new BuiltInTypes()
            {
                Boolean = true,
                Byte = 0x1,
                Char = (char)2,
                Decimal = 3,
                Double = 4.0,
                Int16 = 5,
                Int32 = 6,
                Int64 = 7,
                SByte = 0x8,
                Single = 9.0f,
                UInt16 = 10,
                UInt32 = 11,
                UInt64 = 12,

                String = "13"
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<BuiltInTypes, BuiltInTypes>()
                    //map with custom converter
                    .MapConditionalMember( a => true, () => -1f, a => a.Single, d => d.String, single => single.ToString() )

                    //map same source property to many different targets
                    .MapConditionalMember( a => true, () => 'f', a => a.Char, d => d.Int32 )

                    //same source and destination members: last mapping overrides and adds the converter 
                    .MapConditionalMember( a => true, () => 'f', a => a.Char, d => d.Single )
                    .MapConditionalMember( a => true, () => -1, a => 123, d => d.Single )
                    .MapConditionalMember( a => true, () => "", a => a.String, d => d.Single )
                    .MapConditionalMember( a => true, () => "", a => a.String, d => d.Single, @string => Single.Parse( @string ) )

                    //same sourceproperty/destinationProperty: second mapping overrides and removes (set to null) the converter
                    .MapConditionalMember( a => true, () => -1f, a => a.Single, y => y.Double, a => a + 254 )
                    .MapConditionalMember( a => true, () => -1f, a => a.Single, y => y.Double );
            } );

            var target = ultraMapper.Map( source );

            Assert.IsTrue( target.Boolean );
            Assert.IsTrue( target.Byte == 0x1 );
            Assert.IsTrue( target.Char == (char)2 );
            Assert.IsTrue( target.Decimal == 3 );
            Assert.IsTrue( target.Double == 9.0 );
            Assert.IsTrue( target.Int16 == 5 );
            Assert.IsTrue( target.Int32 == 2 );
            Assert.IsTrue( target.Int64 == 7 );
            Assert.IsTrue( target.SByte == 0x8 );
            Assert.IsTrue( target.Single == 13 );
            Assert.IsTrue( target.UInt16 == 10 );
            Assert.IsTrue( target.UInt32 == 11 );
            Assert.IsTrue( target.UInt64 == 12 );
        }



        [TestMethod]
        public void NullablePrimitiveTypeToADifferentPrimitiveType()
        {
            var source = new NullablePrimitiveTypes();
            var target = new BuiltInTypes();

            var ultraMapper = new Mapper
            (
                cfg =>
                {
                    cfg.MapTypes<NullablePrimitiveTypes, BuiltInTypes>( tmc => tmc.IgnoreMemberMappingResolvedByConvention = true )
                       .MapMember( s => s.Int32, t => t.Char );
                }
            );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void PrimitiveTypeToADifferentNullablePrimitiveType()
        {
            var source = new BuiltInTypes();
            var target = new NullablePrimitiveTypes();

            var ultraMapper = new Mapper
            (
                cfg =>
                {
                    cfg.MapTypes<BuiltInTypes, NullablePrimitiveTypes>( tmc => tmc.IgnoreMemberMappingResolvedByConvention = true )
                       .MapMember( s => s.Int32, t => t.Char );
                }
            );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void BuiltInToNullable()
        {
            var source = new BuiltInTypes();
            var target = new NullableBuiltInTypes();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void NullableToNullable()
        {
            var source = new NullableBuiltInTypes();
            var target = new NullableBuiltInTypes();

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
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
                SByte = 0x23,
                Single = 24,
                String = "25",
                UInt16 = 26,
                UInt32 = 27,
                UInt64 = 28
            };

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
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

            var ultraMapper = new Mapper();
            ultraMapper.Map( source, target );

            var isResultOk = ultraMapper.VerifyMapperResult( source, target );
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

        //    var ultraMapper = new UltraMapper
        //    (
        //        cfg =>
        //        {
        //            cfg.GlobalConfiguration.IgnoreConventions = true;

        //            cfg.MapTypes<NullableBuiltInTypes, BuiltInTypes>()
        //                .MapProperty( s => s.Int32, s => s.Char );
        //        }
        //    );

        //    ultraMapper.Map( source, target );
        //    ultraMapper.VerifyMapperResult( source, target );
        //}
    }
}

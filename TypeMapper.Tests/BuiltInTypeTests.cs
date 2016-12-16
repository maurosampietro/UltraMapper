using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;

namespace TypeMapper.Tests
{
    [TestClass]
    public class BuiltInTypeTests
    {
        private class BuiltInTypes
        {
            public bool Boolean { get; set; } = true;
            public byte Byte { get; set; } = 0x1;
            public char Char { get; set; } = 'a';
            public decimal Decimal { get; set; } = 3;
            public double Double { get; set; } = 4.0;
            public short Int16 { get; set; } = 10;
            public int Int32 { get; set; } = 6;
            public long Int64 { get; set; } = 8;
            public object Object { get; set; } = null;
            public sbyte SByte { get; set; } = 0x2;
            public float Single { get; set; } = 5.0f;
            public string String { get; set; } = "12";
            public ushort UInt16 { get; set; } = 11;
            public uint UInt32 { get; set; } = 7;
            public ulong UInt64 { get; set; } = 9;
        }

        private class BuiltInTypesDto
        {
            public bool Boolean { get; set; }
            public byte Byte { get; set; }
            public char Char { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public short Int16 { get; set; }
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public object Object { get; set; }
            public sbyte SByte { get; set; }
            public float Single { get; set; }
            public string String { get; set; }
            public ushort UInt16 { get; set; }
            public uint UInt32 { get; set; }
            public ulong UInt64 { get; set; }
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

        private class NullablePrimitiveTypesDto
        {
            public bool? Boolean { get; set; }
            public byte? Byte { get; set; }
            public char? Char { get; set; }
            public decimal? Decimal { get; set; }
            public double? Double { get; set; }
            public float? Float { get; set; }
            public short? Int16 { get; set; }
            public int? Int32 { get; set; }
            public long? Int64 { get; set; }
            public sbyte? SByte { get; set; }
            public ushort? UInt16 { get; set; }
            public uint? UInt32 { get; set; }
            public ulong? UInt64 { get; set; }
        }

        [TestMethod]
        public void BuiltInTest()
        {
            var temp = new BuiltInTypes();
            var temp2 = new BuiltInTypesDto();

            var config = new TypeConfiguration();
            config.MapTypes<BuiltInTypes, BuiltInTypesDto>()
                //map with custom converter
                .MapProperty( a => a.Single, d => d.String, single => single.ToString() )

                //map same source property to many different targets
                .MapProperty( a => a.Char, d => d.Single )
                .MapProperty( a => a.Char, d => d.Int32 )

                //same sourceproperty/destinationProperty: second mapping overrides and adds the converter 
                .MapProperty( a => a.String, d => d.Single )
                .MapProperty( a => a.String, d => d.Single, @string => Single.Parse( @string ) )

                //same sourceproperty/destinationProperty: second mapping overrides and removes (set to null) the converter
                .MapProperty( a => a.Single, y => y.Double, a => a + 254 )
                .MapProperty( a => a.Single, y => y.Double );

            var typeMapper = new TypeMapper( config );
            typeMapper.Map( temp, temp2 );
        }

        [TestMethod]
        public void NullableToNullableTest()
        {
            var source = new NullablePrimitiveTypes();
            var target = new NullablePrimitiveTypesDto();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );
        }

        [TestMethod]
        public void BuiltInToNullableTypes()
        {
            var source = new BuiltInTypes();
            var target = new NullablePrimitiveTypesDto();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );
        }

        [TestMethod]
        public void NullableToBuiltInTypes()
        {
            var soruce = new NullablePrimitiveTypes();
            var target = new BuiltInTypesDto()
            {
                Boolean = true,
                Byte = 0x1,
                Char = 'a',
                Decimal = 3,
                Double = 4.0,
                Int16 = 10,
                Int32 = 6,
                Int64 = 8,
                Object = null,
                SByte = 0x2,
                Single = 5.0f,
                String = "12",
                UInt16 = 11,
                UInt32 = 7,
                UInt64 = 9,
            };

            var typeMapper = new TypeMapper();
            typeMapper.Map( soruce, target );
        }
    }
}

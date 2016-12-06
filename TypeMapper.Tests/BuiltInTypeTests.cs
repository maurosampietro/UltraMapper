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
        private class BaseTypes
        {
            public long NotImplicitlyConvertible { get; set; } = 31;
            public int ImplicitlyConvertible { get; set; } = 33;
            public bool Boolean { get; set; } = true;
            public byte Byte { get; set; } = 0x1;
            public sbyte SByte { get; set; } = 0x2;
            public char Char { get; set; } = 'a';
            public decimal Decimal { get; set; } = 3;
            public double Double { get; set; } = 4.0;
            public float Single { get; set; } = 5.0f;
            public int Int32 { get; set; } = 6;
            public uint UInt32 { get; set; } = 7;
            public long Int64 { get; set; } = 8;
            public ulong UInt64 { get; set; } = 9;
            public object Object { get; set; } = null;
            public short Int16 { get; set; } = 10;
            public ushort UInt16 { get; set; } = 11;
            public string String { get; set; } = "12";
            public int? NullableInt32 { get; set; } = 12;
            public int? NullNullableInt32 { get; set; } = null;
        }

        private class BaseTypesDto
        {
            public int NotImplicitlyConvertible { get; set; }
            public long ImplicitlyConvertible { get; set; }
            public bool Boolean { get; set; }
            public byte Byte { get; set; }
            public sbyte SByte { get; set; }
            public char Char { get; set; }
            public decimal Decimal { get; set; }
            public double Double { get; set; }
            public float Single { get; set; }
            public int Int32 { get; set; }
            public uint UInt32 { get; set; }
            public long Int64 { get; set; }
            public ulong UInt64 { get; set; }
            public object Object { get; set; }
            public short Int16 { get; set; }
            public ushort UInt16 { get; set; }
            public string String { get; set; }
            public int? NullableInt32 { get; set; }
        }

        [TestMethod]
        public void BuiltInTypesTest()
        {
            var temp = new BaseTypes();
            var temp2 = new BaseTypesDto();

            var config = new TypeConfiguration();
            config.MapTypes<BaseTypes, BaseTypesDto>()
                .MapProperty( a => a.String, d => d.Single )
                .MapProperty( a => a.String, d => d.Single, @string => Single.Parse( @string ) )
                .MapProperty( a => a.Single, d => d.String, single => single.ToString() )

                //same sourceproperty/destinationProperty: second mapping overrides 
                .MapProperty( a => a.Single, y => y.Double, a => a + 254 )
                .MapProperty( a => a.Single, y => y.Double )

                .MapProperty( a => a.NullableInt32, d => d.Char )
                .MapProperty( a => a.Char, d => d.NullableInt32 )
                .MapProperty( a => a.Char, d => d.Int32 );

            var typeMapper = new TypeMapper( config );
            typeMapper.Map( temp, temp2 );
        }
    }
}

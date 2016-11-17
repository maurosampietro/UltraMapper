using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;

namespace TypeMapper.Tests
{
    [TestClass]
    public class TypeMapperTests
    {
        public class BaseTypes
        {
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

            public InnerType InnerType { get; set; }
            public int? NullableInt32 { get; set; } = 12;
            public int? NullNullableInt32 { get; set; } = null;

            public BaseTypes SelfReference { get; set; }
            public BaseTypes Reference { get; set; }

            public List<int> ListOfInts { get; set; }

            public BaseTypes()
            {
                this.SelfReference = this;
                this.InnerType = new InnerType() { A = "vara", B = "varb", C = this };

                this.ListOfInts = new List<int>() { 1, 2, 3 };
            }
        }

        public class BaseTypesDto
        {
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

            public InnerTypeDto InnerType { get; set; }
            public BaseTypesDto SelfReference { get; set; }

            public BaseTypes Reference { get; set; }

            public List<int> ListOfInts { get; set; }
        }

        public class InnerType
        {
            public string A { get; set; }
            public string B { get; set; }

            public BaseTypes C { get; set; }
        }

        public class InnerTypeDto
        {
            public string A { get; set; }
            public string B { get; set; }

            public BaseTypes C { get; set; }
        }


        [TestMethod]
        public void SimpleTypes()
        {
            var temp = new BaseTypes();

            var temp2 = new BaseTypesDto();
            temp2.Reference = temp;

            var config = new TypeConfiguration();

            var config2 = config.Map<BaseTypes, BaseTypesDto>()

                 //null nullable
                 .MapProperty( a => a.NullNullableInt32, b => b.SByte )
                 //.MapProperty( a => a.NullNullableInt32, b => b.SByte, a => a == null ? 0 : a.Value )
                 .MapProperty( a => a.NullNullableInt32, b => b.Int32, a => a == null ? 0 : a.Value )

                 //inner class
                 .MapProperty( a => a.InnerType, b => b.InnerType )

            //circular reference (self reference)
            .MapProperty( a => a.SelfReference, b => b.SelfReference )

            .MapProperty( a => a.String, d => d.String )
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

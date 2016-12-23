using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using TypeMapper.MappingConventions;
using TypeMapper.Configuration;
using TypeMapper.Mappers;
using TypeMapper.CollectionMappingStrategies;
using System.Linq;

namespace TypeMapper.Tests
{
    [TestClass]
    public class TypeMapperTests
    {
        public class BaseTypes
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

            public InnerType InnerType { get; set; }
            public BaseTypes SelfReference { get; set; }
            public BaseTypes Reference { get; set; }

            //public List<int> ListOfInts { get; set; }
            //public List<InnerType> ListOfInnerType { get; set; }
            //public Dictionary<string, int> DictionaryBuiltInTypes { get; set; }
            //public Dictionary<InnerType, InnerType> Dictionary { get; set; }

            public BaseTypes()
            {
                this.SelfReference = this;
                this.InnerType = new InnerType() { A = "vara", B = "varb", C = this };

                //this.ListOfInts = new List<int>( Enumerable.Range( 1, (int)Math.Pow( 10, 2 ) ) );

                //this.ListOfInnerType = new List<Tests.TypeMapperTests.InnerType>() {
                //    new Tests.TypeMapperTests.InnerType() { A = "a", B="b",C = this  },
                //    new Tests.TypeMapperTests.InnerType(){ A = "c", B="d",C = this  },
                //};

                //this.DictionaryBuiltInTypes = new Dictionary<string, int>()
                //{
                //    {"a",1}, {"b",2}, {"c",3}
                //};

                //this.Dictionary = new Dictionary<InnerType, InnerType>()
                //{
                //    {new InnerType() { A= "aa" }, new InnerType() { A= "ab" }},
                //    {new InnerType() { B= "ba" }, new InnerType() { B= "bb" }},
                //    {new InnerType() { A= "ca" }, new InnerType() { A= "cb" }},
                //};
            }
        }

        public class BaseTypesDto
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

            public InnerTypeDto InnerType { get; set; }
            public BaseTypesDto SelfReference { get; set; }

            public BaseTypes Reference { get; set; }

            //public List<int> ListOfInts { get; set; }

            //public BindingList<InnerTypeDto> ListOfInnerTypeDto { get; set; }

            //public Dictionary<string, int> DictionaryBuiltInTypes { get; set; }
            //public Dictionary<InnerTypeDto, InnerTypeDto> Dictionary { get; set; }

            public BaseTypesDto()
            {

                //this.ListOfInts = new List<int>() { 0 };
            }
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

            var config = new MapperConfiguration();

            var mapper = new TypeMapper<DefaultMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.Mappers.Add<BuiltInTypeMapper>()
                    .Add<ReferenceMapper>();
                    //.Add<CollectionMapper>()
                    //.Add<DictionaryMapper>();

                cfg.GlobalConfiguration.MappingConvention.PropertyMatchingRules
                    //.GetOrAdd<TypeMatchingRule>( ruleConfig => ruleConfig.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( ruleConfig => ruleConfig.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( ruleConfig => ruleConfig.IgnoreCase = true )
                    .Respect( ( rule2, rule3 ) => (rule2 | rule3) );

                cfg.MapTypes<BaseTypes, BaseTypesDto>()
                   //.MapProperty( a => a.Dictionary, b => b.Dictionary )

                   //.MapProperty( a => a.ListOfInts, b => b.ListOfInts, new NewCollection() )
                   //.MapProperty( a => a.ListOfInnerType, b => b.ListOfInnerTypeDto )
                   ////null nullable
                   //.MapProperty( a => a.NullNullableInt32, b => b.SByte )
                   ////.MapProperty( a => a.NullNullableInt32, b => b.SByte, a => a == null ? 0 : a.Value )
                   //.MapProperty( a => a.NullNullableInt32, b => b.Int32, a => a == null ? 0 : a.Value )

                   ////inner class
                   //.MapProperty( a => a.InnerType, b => b.String, c => c.A + c.B )
                   ////.MapProperty( a => a.InnerType, b => b.InnerType )

                   ////circular reference (self reference)
                   //.MapProperty( a => a.SelfReference, b => b.SelfReference )

                   .MapProperty( a => a.String, d => d.Single )
                   .MapProperty( a => a.String, d => d.Single, @string => Single.Parse( @string ) )
                   //.MapProperty( a => a.String, d => d.InnerType.A ) //subnavigation and flattening not supported
                   .MapProperty( a => a.Single, d => d.String, single => single.ToString() )

                   //same sourceproperty/destinationProperty: second mapping overrides 
                   .MapProperty( a => a.Single, y => y.Double, a => a + 254 )
                   .MapProperty( a => a.Single, y => y.Double )

                   .MapProperty( a => a.NullableInt32, d => d.Char )
                   .MapProperty( a => a.Char, d => d.NullableInt32 )
                   .MapProperty( a => a.Char, d => d.Int32 );
            } );

            mapper.Map( temp, temp2 );
        }
    }

    [TestClass]
    public class ConventionTests
    {
        public class SourceClass
        {
            public int A { get; set; } = 31;
            public long B { get; set; } = 33;
            public int C { get; set; } = 71;
            public int D { get; set; } = 73;
            public int E { get; set; } = 101;
        }

        public class TargetClass
        {
            public long A { get; set; }
            public int B { get; set; }
        }

        public class TargetClassDto
        {
            public long ADto { get; set; }
            public int ADataTransferObject { get; set; }

            public long BDataTransferObject { get; set; }

            public int Cdto { get; set; }
            public int Ddatatransferobject { get; set; }

            public int E { get; set; }
        }

        //[TestMethod]
        //public void ExactNameAndImplicitlyConvertibleTypeConventionTest()
        //{
        //    var source = new SourceClass();
        //    var target = new TargetClass();

        //    var mapper = new TypeMapper();
        //    mapper.Map( source, target );

        //    Assert.IsTrue( source.A == target.A );
        //    Assert.IsTrue( source.B != target.B );
        //}

        [TestMethod]
        public void ExactNameAndTypeConventionTest()
        {
            var source = new SourceClass();
            var target = new TargetClass();

            var config = new MapperConfiguration( cfg =>
            {
                cfg.PropertyMatchingRules.GetOrAdd<TypeMatchingRule>( ruleConfig =>
                {
                    ruleConfig.AllowImplicitConversions = false;
                    ruleConfig.AllowExplicitConversions = false;
                } );
            } );

            var mapper = new TypeMapper( config );
            mapper.Map( source, target );

            Assert.IsTrue( source.A != target.A );
            Assert.IsTrue( source.B != target.B );
        }

        [TestMethod]
        public void SuffixNameAndTypeConventionTest()
        {
            var source = new SourceClass();
            var target = new TargetClassDto();

            var mapper = new TypeMapper<DefaultMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.Mappers.Add<BuiltInTypeMapper>()
                    .Add<ReferenceMapper>();
                    //.Add<CollectionMapper>()
                    //.Add<DictionaryMapper>();

                cfg.GlobalConfiguration.MappingConvention.PropertyMatchingRules
                    .GetOrAdd<TypeMatchingRule>( ruleConfig => ruleConfig.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( ruleConfig => ruleConfig.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( ruleConfig => ruleConfig.IgnoreCase = true )
                    .Respect( ( rule1, rule2, rule3 ) => rule1 & (rule2 | rule3) );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( source.A == target.ADto );
            Assert.IsTrue( source.B == target.BDataTransferObject );
            Assert.IsTrue( source.C == target.Cdto );
            Assert.IsTrue( source.D == target.Ddatatransferobject );
            Assert.IsTrue( source.E == target.E );
        }
    }
}

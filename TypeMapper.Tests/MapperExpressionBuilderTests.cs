using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Internals;
using TypeMapper.Mappers;

namespace TypeMapper.Tests
{
    [TestClass]
    public class MapperExpressionBuilderTests
    {
        private class OuterType
        {
            public InnerType NullInnerType { get; set; }
            public InnerType InnerType { get; set; }
            public List<int> PrimitiveList { get; set; }
            public List<InnerType> ComplexList { get; set; }

            public string String { get; set; }
        }

        private class OuterTypeDto
        {
            public InnerTypeDto NullInnerTypeDto { get; set; }
            public InnerTypeDto InnerTypeDto { get; set; }

            public OuterTypeDto()
            {
                this.NullInnerTypeDto = new InnerTypeDto();
                this.InnerTypeDto = new InnerTypeDto();
            }
        }

        private class InnerType
        {
            public string A { get; set; }
            public string B { get; set; }

            //public OuterType C { get; set; }
        }

        private class InnerTypeDto
        {
            public string A { get; set; }
            public string B { get; set; }

            public OuterType C { get; set; }
        }

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
            public string String { get; set; } = "14";
        }

        [TestMethod]
        public void ReferenceMapperTest()
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

            var mappingExpression = new ReferenceMapper( typeMapper.MappingConfiguration )
                .GetMappingExpression( source.GetType(), target.GetType() );

            var refTracking = new ReferenceTracking();
            var func = (Func<ReferenceTracking, BuiltInTypes, BuiltInTypes, List<ObjectPair>>)
                mappingExpression.Compile();

            func( refTracking, source, target );

            typeMapper.MappingConfiguration[ source.GetType(), target.GetType() ]
                .MapperFunc( new ReferenceTracking(), source, target );


            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ReferenceMapperTest2()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var source = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            //source.InnerType.C = source;

            var target = new OuterType();
            var typeMapper = new TypeMapper();

            var mappingExpression = new ReferenceMapper( typeMapper.MappingConfiguration )
                .GetMappingExpression( source.GetType(), target.GetType() );

            var refTracking = new ReferenceTracking();
            var func = (Func<ReferenceTracking, OuterType, OuterType, List<ObjectPair>>)
                mappingExpression.Compile();

            func( refTracking, source, target );

            typeMapper.MappingConfiguration[ source.GetType(), target.GetType() ]
                .MapperFunc( new ReferenceTracking(), source, target );


            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

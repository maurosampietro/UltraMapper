using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper;
using TypeMapper.MappingConventions;

namespace ConsoleApplication
{
    class Program
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

            public BaseTypes()
            {
                //this.SelfReference = this;
                this.InnerType = new InnerType() { A = "vara", B = "varb", C = this };
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



        static void Main( string[] args )
        {
            var temp = new BaseTypes();
            var temp2 = new BaseTypesDto();

            //var p = typeof( BaseTypes ).GetProperty( "Int32" );
            //var setter1 = FastInvoke.BuildUntypedSetter<BaseTypes, int>( p );
            //var setter2 = FastInvoke.BuildUntypedSetter<BaseTypes>( p );
            //var setter3 = FastInvoke.BuildUntypedCastSetter( p );

            //Stopwatch sw1 = new Stopwatch();
            //sw1.Start();
            //for( int i = 0; i < 10000000; i++ )
            //{
            //    setter1( temp, i );
            //}
            //sw1.Stop();
            //Console.WriteLine( sw1.ElapsedMilliseconds );

            //Stopwatch sw2 = new Stopwatch();
            //sw2.Start();
            //for( int i = 0; i < 10000000; i++ )
            //{
            //    setter2( temp, i );
            //}
            //sw2.Stop();
            //Console.WriteLine( sw2.ElapsedMilliseconds );

            //Stopwatch sw3 = new Stopwatch();
            //sw3.Start();
            //for( int i = 0; i < 10000000; i++ )
            //{
            //    setter3( temp, i );
            //}
            //sw3.Stop();
            //Console.WriteLine( sw3.ElapsedMilliseconds );

            //var mapper = new TypeMapper.TypeMapper();
            //Stopwatch sw4 = new Stopwatch();
            //sw4.Start();
            //for( int i = 0; i < 10000000; i++ )
            //{
            //    mapper.Map( temp, temp2 );
            //}
            //sw4.Stop();
            //Console.WriteLine( sw4.ElapsedMilliseconds );

            //Stopwatch sw5 = new Stopwatch();
            //sw5.Start();
            //AutoMapper.Mapper.Initialize( cfg => cfg.CreateMissingTypeMaps = true );
            //for( int i = 0; i < 10000000; i++ )
            //{
            //    AutoMapper.Mapper.Map( temp, temp2 );
            //}
            //sw5.Stop();
            //Console.WriteLine( sw5.ElapsedMilliseconds );

            var type = typeof( KeyValuePair<string, int> );

            Stopwatch sw4 = new Stopwatch();
            sw4.Start();
            var instanceCreator4 = ConstructorFactory.GetOrCreateConstructor( type );
            for( int i = 0; i < 10000000; i++ )
            {
                instanceCreator4( );
                //InstanceFactoryNoCasting.GetInstance<KeyValuePair<string, int>>();
            }
            sw4.Stop();
            Console.WriteLine( "return object " + sw4.ElapsedMilliseconds );


            Stopwatch sw5 = new Stopwatch();
            sw5.Start();
            var instanceCreator5 = ConstructorFactory.GetOrCreateConstructor<KeyValuePair<string, int>>();
            for( int i = 0; i < 10000000; i++ )
            {
                instanceCreator5();
                //InstanceFactoryNoCasting.GetInstance<KeyValuePair<string, int>>();
            }
            sw5.Stop();
            Console.WriteLine( "cast everything " + sw5.ElapsedMilliseconds );

            Stopwatch sw6 = new Stopwatch();
            sw6.Start();
            var instanceCreator6 = ConstructorFactory.GetOrCreateConstructor<string, int>( type );
            for( int i = 0; i < 10000000; i++ )
            {
                instanceCreator6( "mauro", i );
                //InstanceFactoryNoCasting.GetInstance<KeyValuePair<string, int>>();
            }
            sw6.Stop();
            Console.WriteLine( "return cast to object" + sw6.ElapsedMilliseconds );

            Stopwatch sw7 = new Stopwatch();
            sw7.Start();

            for( int i = 0; i < 10000000; i++ )
            {
                new KeyValuePair<string, int>( "mauro", i );
            }
            sw7.Stop();
            Console.WriteLine( "new " + sw7.ElapsedMilliseconds );

            Stopwatch sw8 = new Stopwatch();
            sw8.Start();
            var instanceCreator8 = ConstructorFactory.GetOrCreateConstructor<string, int, KeyValuePair<string, int>>();
            for( int i = 0; i < 10000000; i++ )
            {
                instanceCreator8( "mauro", i );
            }
            sw8.Stop();
            Console.WriteLine( "no cast delegate" + sw8.ElapsedMilliseconds );

            Stopwatch sw9 = new Stopwatch();
            sw9.Start();
            var instanceCreator9 = ConstructorFactory.GetOrCreateConstructor<string, int, KeyValuePair<string, int>>();
            for( int i = 0; i < 10000000; i++ )
            {
                Activator.CreateInstance( type );
            }
            sw9.Stop();
            Console.WriteLine( "Activator" + sw9.ElapsedMilliseconds );

            Console.ReadKey();
        }
    }
}

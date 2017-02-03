using Akka.Actor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TypeMapper;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace ConsoleApplication
{
    class Program
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

            public List<int> ListOfInts { get; set; }
            public List<InnerType> ListOfInnerType { get; set; }

            public Dictionary<string, int> DictionaryBuiltInTypes { get; set; }
            public Dictionary<InnerType, InnerType> Dictionary { get; set; }
            public Dictionary<InnerType, InnerTypeDto> Dictionary2 { get; set; }
            public Dictionary<string, InnerType> Dictionary3 { get; set; }
            public Dictionary<InnerType, int> Dictionary4 { get; set; }

            public BaseTypes()
            {
                this.SelfReference = this;
                this.InnerType = new InnerType() { A = "vara", B = "varb" };

                this.ListOfInts = new List<int>( Enumerable.Range( 1, (int)Math.Pow( 10, 2 ) ) );

                this.ListOfInnerType = new List<InnerType>() {
                    new InnerType() { A = "a", B="b", },
                    new InnerType(){ A = "c", B="d", },
                };

                this.DictionaryBuiltInTypes = new Dictionary<string, int>()
                {
                    {"a",1}, {"b",2}, {"c",3}
                };

                this.Dictionary = new Dictionary<InnerType, InnerType>()
                {
                    {new InnerType() { A= "aa" }, new InnerType() { A= "ab" }},
                    {new InnerType() { B= "ba" }, new InnerType() { B= "bb" }},
                    {new InnerType() { A= "ca" }, new InnerType() { A= "cb" }},
                };

                this.Dictionary2 = new Dictionary<InnerType, InnerTypeDto>()
                {
                    {new InnerType() { A= "aa" }, new InnerTypeDto() { A= "ab" }},
                    {new InnerType() { B= "ba" }, new InnerTypeDto() { B= "bb" }},
                    {new InnerType() { A= "ca" }, new InnerTypeDto() { A= "cb" }},
                };

                this.Dictionary3 = new Dictionary<string, InnerType>()
                {
                    {"aa", new InnerType() { A= "ab" }},
                    {"ba", new InnerType() { B= "bb" }},
                    {"ca", new InnerType() { A= "cb" }},
                };

                this.Dictionary4 = new Dictionary<InnerType, int>()
                {
                    {new InnerType() { A= "aa" }, 1},
                    {new InnerType() { B= "ba" }, 2},
                    {new InnerType() { A= "ca" }, 3},
                };
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

            public List<int> ListOfInts { get; set; }

            public BindingList<InnerTypeDto> ListOfInnerType { get; set; }

            //public Dictionary<string, int> DictionaryBuiltInTypes { get; set; }
            //public Dictionary<InnerTypeDto, InnerTypeDto> Dictionary { get; set; }
            //public Dictionary<InnerTypeDto, InnerType> Dictionary2 { get; set; }
            //public Dictionary<string, InnerTypeDto> Dictionary3 { get; set; }
            //public Dictionary<InnerTypeDto, int> Dictionary4 { get; set; }
        }

        public class InnerType
        {
            public string A { get; set; }
            public string B { get; set; }

            public BaseTypes C { get; set; }
            public InnerType D { get; set; }
        }

        public class InnerTypeDto
        {
            public string A { get; set; }
            public string B { get; set; }

            public BaseTypesDto C { get; set; }
            public InnerTypeDto D { get; set; }
        }

        static void Main( string[] args )
        {
            AutoMapper.Mapper.Initialize( cfg => { } );

            int v = 10;
            int w = 13;

            AutoMapper.Mapper.Map( v, w );

            List<int> a = Enumerable.Range( 0, 10 ).ToList();
            List<double?> b = new List<double?>() { null, null, null };

            AutoMapper.Mapper.Map( b, a );

            var temp = new BaseTypes();
            var temp2 = new BaseTypesDto();

            int iterations = (int)Math.Pow( 10, 6 );

            var mapper = new TypeMapper<CustomMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.MappingConvention.PropertyMatchingRules
                    //.GetOrAdd<TypeMatchingRule>( rule => rule.AllowImplicitConversions = true )
                    .GetOrAdd<ExactNameMatching>( rule => rule.IgnoreCase = true )
                    .GetOrAdd<SuffixMatching>( rule => rule.IgnoreCase = true )
                    .Respect( ( /*rule1,*/ rule2, rule3 ) => /*rule1 & */(rule2 | rule3) );
            } );

            Stopwatch sw4 = new Stopwatch();
            sw4.Start();
            for( int i = 0; i < iterations; i++ )
            {
                mapper.Map( temp, temp2 );
            }
            sw4.Stop();
            Console.WriteLine( sw4.ElapsedMilliseconds );

            //var exp = mapper._mappingConfiguration[ typeof( BaseTypes ), typeof( BaseTypesDto ) ].First().Expression;
            //var func = (Func<ReferenceTracking, BaseTypes, BaseTypesDto, IEnumerable<ObjectPair>>)exp.Compile();
            //func( new ReferenceTracking(), temp, temp2 );

            Stopwatch sw5 = new Stopwatch();

            var temp3 = new BaseTypes();
            var temp4 = new BaseTypesDto();

            AutoMapper.Mapper.Initialize( cfg =>
            {
                cfg.CreateMissingTypeMaps = true;
                cfg.CreateMap<BaseTypes, BaseTypesDto>().PreserveReferences();
            } );
            sw5.Start();
            for( int i = 0; i < iterations; i++ )
            {
                AutoMapper.Mapper.Map( temp3, temp4 );
            }
            sw5.Stop();
            Console.WriteLine( sw5.ElapsedMilliseconds );

            Console.ReadKey();
        }

        //// Create an (immutable) message type that your actor will respond to
        //public class Greet
        //{
        //    public Greet( string who )
        //    {
        //        Who = who;
        //    }
        //    public string Who { get; private set; }
        //}

        //// Create the actor class
        //public class GreetingActor : ReceiveActor
        //{
        //    public static int i = 0;
        //    private List<int> threads = new List<int>();

        //    public GreetingActor()
        //    {
        //        // Tell the actor to respond to the Greet message
        //        Receive<Greet>( greet =>
        //        {
        //            int currentThread = Thread.CurrentThread.ManagedThreadId;
        //            if( !threads.Contains( currentThread ) )
        //                threads.Add( currentThread );

        //            Console.SetCursorPosition( 0, threads.IndexOf( currentThread ) );
        //            Console.WriteLine( $"{currentThread}: Hello {greet.Who}" );
        //            i++;
        //        } );
        //    }
        //}

        //private static void InitActor()
        //{
        //    // Create a new actor system (a container for your actors)
        //    var system = ActorSystem.Create( "MySystem" );

        //    // Create your actor and get a reference to it.
        //    // This will be an "ActorRef", which is not a
        //    // reference to the actual actor instance
        //    // but rather a client or proxy to it.
        //    var greeter = system.ActorOf<GreetingActor>( "greeter" );

        //    // Send a message to the actor
        //    Parallel.For( 0, 10000, ( i ) =>
        //    {
        //        greeter.Tell( new Greet( "World_" + i ) );
        //    } );


        //    // This prevents the app from exiting
        //    // before the async work is done
        //    Console.ReadLine();  
        //}
    }
}

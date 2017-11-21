using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class InheritTypeMappingTests
    {
        public class TestClass
        {
            public bool Boolean { get; set; }
            public string String { get; set; }
            public List<string> Strings { get; set; } = new List<string>();
            public List<bool> Booleans { get; set; } = new List<bool>();
        }

        public class SubTestClass : TestClass
        {
            public int Integer { get; set; }
        }

        public class Container
        {
            public TestClass TestClass { get; set; }
        }

        [TestMethod]
        public void InheritMapping()
        {
            var source = new TestClass();
            source.Strings.Clear();
            source.Booleans.Clear();

            source.Booleans.Add( true );
            source.Booleans.Add( false );

            var target = new TestClass();

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<bool, string>( b => b ? "1" : "0" );

                cfg.MapTypes<TestClass, TestClass>()
                    .MapMember( a => a.Boolean, y => y.String )
                    .MapMember( a => a.Booleans, y => y.Strings );
            } );

            ultraMapper.Map( source, target );

            Assert.IsTrue( source.Boolean ? target.String == "1"
                : target.String == "0" );

            Assert.IsTrue( target.Strings.Contains( "1" ) &&
                target.String.Contains( "0" ) );
        }

        /// <summary>
        /// Should not map to the declared type,
        /// but to the runtime used type which could be 
        /// a type inherited/derived from the declared type
        /// </summary>

        //[TestMethod]
        //public void ShouldMapToRuntimeUsedType()
        //{
        //    var source = new Container()
        //    {
        //        TestClass = new SubTestClass()
        //        {
        //            Boolean = true,
        //            String = "ciao",
        //            //Integer = 11
        //        }
        //    };

        //    //scrivere di questo su github: automapper non mappa niente che non sia 
        //    //esplicitamente dichiarato con CreateMap.
        //    //CreateMissingTypeMaps non so a cosa serve visto che settarlo a true non cambia le cose.
        //    //Automapper copia solo il riferimento di ogni cosa non venga esplicitamente mappato.
        //    //Se viene mappato un oggetto contenitore, il senso comune dice che tutto
        //    //ciò che vi è all'interno dovrebbe essere mappato automaticamente.
        //    //Buona fortuna a mappare oggetti complessi che coinvolgono molti tipi.
        //    AutoMapper.Mapper.Initialize(cfg =>
        //    {
        //        cfg.CreateMissingTypeMaps = true;
        //        cfg.CreateMap<TestClass, TestClass>();
        //        cfg.CreateMap<Container, Container>();
        //    } );

        //    //var configuration = new AutoMapper.MapperConfiguration( 
        //    //var executionPlan = configuration.BuildExecutionPlan( typeof( Container ), typeof( Container ) );

        //    var target2 = AutoMapper.Mapper.Map<Container>( source );
        //    Assert.IsTrue( !Object.ReferenceEquals( source.TestClass, target2.TestClass ) );

        //    var target = new Container();

        //    var ultraMapper = new Mapper();
        //    ultraMapper.Map( source, target );

        //    Assert.IsTrue( target.TestClass.GetType() == source.TestClass.GetType() );

        //    var isResultOk = ultraMapper.VerifyMapperResult( source, target );
        //    Assert.IsTrue( isResultOk );
        //}
    }
}

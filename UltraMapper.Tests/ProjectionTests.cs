using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.Tests
{
    [TestClass]
    public class ProjectionTests
    {
        private class FirstLevel
        {
            public string A { get; set; }
            public string A1 { get; set; }
            public string A2 { get; set; }

            public SecondLevel SecondLevel { get; set; }

            public SecondLevel GetSecond() { return SecondLevel; }
        }

        private class SecondLevel
        {
            public string A { get; set; }
            public ThirdLevel ThirdLevel { get; set; }

            public ThirdLevel GetThird() { return this.ThirdLevel; }
            public ThirdLevel SetThird( ThirdLevel value ) { return this.ThirdLevel = value; }
        }

        private class ThirdLevel
        {
            public string A { get; set; }

            public void SetA( string value )
            {
                this.A = value;
            }
        }

        [TestMethod]
        public void ManualFlatteningUsingExistingInstances()
        {
            var source = new FirstLevel()
            {
                A = "first",

                SecondLevel = new SecondLevel()
                {
                    A = "second",

                    ThirdLevel = new ThirdLevel()
                    {
                        A = "third"
                    }
                }
            };

            var target = new FirstLevel()
            {
                A = "first",

                SecondLevel = new SecondLevel()
                {
                    A = "second",

                    ThirdLevel = new ThirdLevel()
                    {
                        A = "third"
                    }
                }
            };

            var ultraMapper = new UltraMapper( cfg =>
            {
                cfg.MapTypes<SecondLevel, SecondLevel>( typeConfig =>
                {
                    typeConfig.ReferenceMappingStrategy = ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
                } );

                cfg.MapTypes<FirstLevel, FirstLevel>()
                    //nested property getter: ok
                    .MapMember( a => a.SecondLevel.ThirdLevel.A, b => b.A )
                    //nested mixed member-type getter: ok
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.A1 )
                    //nested multiple method getter
                    .MapMember( a => a.GetSecond().GetThird().A, b => b.A2 )
                    //nested mixed member-type getter and setter method: ok
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.SecondLevel.GetThird().A,
                        ( b, value ) => b.SecondLevel.GetThird().SetA( value ) );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ManualFlatteningWithoutUsingExistingInstances()
        {
            var source = new FirstLevel()
            {
                A = "first",

                SecondLevel = new SecondLevel()
                {
                    A = "second",

                    ThirdLevel = new ThirdLevel()
                    {
                        A = "third"
                    }
                }
            };

            var target = new FirstLevel()
            //{
            //    A = "first",

            //    SecondLevel = new SecondLevel()
            //    {
            //        A = "second",

            //        ThirdLevel = new ThirdLevel()
            //        {
            //            A = "third"
            //        }
            //    }
            //}
            ;

            var ultraMapper = new UltraMapper( cfg =>
            {
                cfg.MapTypes<SecondLevel, SecondLevel>( typeConfig =>
                {
                    typeConfig.ReferenceMappingStrategy = ReferenceMappingStrategies.CREATE_NEW_INSTANCE;
                } );

                cfg.MapTypes<FirstLevel, FirstLevel>()
                    //nested property getter: ok
                    .MapMember( a => a.SecondLevel.ThirdLevel.A, b => b.A )
                    //nested mixed member-type getter: ok
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.A1 )
                    //nested multiple method getter
                    .MapMember( a => a.GetSecond().GetThird().A, b => b.A2 )
                    //nested mixed member-type getter and setter method: ok
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.SecondLevel.GetThird().A,
                        ( b, value ) => b.SecondLevel.GetThird().SetA( value ) );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;
using UltraMapper.Internals;

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
            public string A1 { get; set; }
            public ThirdLevel ThirdLevel { get; set; }

            public ThirdLevel GetThird() { return this.ThirdLevel; }
            public ThirdLevel SetThird( ThirdLevel value ) { return this.ThirdLevel = value; }
        }

        private class ThirdLevel
        {
            public string A { get; set; }
            public string A1 { get; set; }

            public void SetA( string value )
            {
                this.A = value;
            }
        }

        [TestMethod]
        public void FlatteningToInstance()
        {
            var source = new FirstLevel()
            {
                A = "first",
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<FirstLevel, object>()
                    .MapMember( a => a.A.Length, b => b );
            } );

            var target = ultraMapper.MapStruct<FirstLevel, int>( source );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
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
                A = "firstTarget",
                A1 = "untouched",

                SecondLevel = new SecondLevel()
                {
                    A = "secondTarget",
                    A1 = "untouched",

                    ThirdLevel = new ThirdLevel()
                    {
                        A = "thirdTarget",
                        A1 = "untouched"
                    }
                }
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<SecondLevel, SecondLevel>( typeConfig =>
                {
                    typeConfig.IgnoreMemberMappingResolvedByConvention = true;
                    typeConfig.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                } )
                .MapMember( s => s.A, t => t.A );

                cfg.MapTypes<FirstLevel, FirstLevel>()
                    .MapMember( a => a.SecondLevel.ThirdLevel.A, b => b.A )
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.A1 )
                    .MapMember( a => a.GetSecond().GetThird().A, b => b.A2 )
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.SecondLevel.GetThird().A,
                        ( b, value ) => b.SecondLevel.GetThird().SetA( value ) );               
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( target.A == "third" );
            Assert.IsTrue( target.A1 == "third" );
            Assert.IsTrue( target.A2 == "third" );

            Assert.IsTrue( isResultOk );
            Assert.IsTrue( target.SecondLevel.A1 == "untouched" );
            Assert.IsTrue( target.SecondLevel.ThirdLevel.A1 == "untouched" );
        }

        [TestMethod]
        public void ManualFlatteningNullSourceMembers()
        {
            var source = new FirstLevel();

            var target = new FirstLevel()
            {
                A = "first",

                SecondLevel = new SecondLevel()
                {
                    A = "suka",

                    ThirdLevel = new ThirdLevel()
                    {
                        A = "suka"
                    }
                }
            };

            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<FirstLevel, FirstLevel>( typeConfig => typeConfig.IgnoreMemberMappingResolvedByConvention = true )
                    .MapMember( a => a.SecondLevel.ThirdLevel.A, b => b.A )
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.A1 )
                    .MapMember( a => a.GetSecond().GetThird().A, b => b.A2 )
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.SecondLevel.GetThird().A,
                        ( b, value ) => b.SecondLevel.GetThird().SetA( value ) );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void ManualFlatteningNullTargetMembers()
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

            var target = new FirstLevel();

            //When target properties are null and you project by setting a target's inner property,
            //you MUST set IgnoreMemberMappingResolvedByConvention = false
            var ultraMapper = new Mapper( cfg =>
            {
                cfg.MapTypes<SecondLevel, SecondLevel>( typeConfig =>
                {
                    typeConfig.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                } );

                cfg.MapTypes<FirstLevel, FirstLevel>( typeConfig => typeConfig.IgnoreMemberMappingResolvedByConvention = false )
                    .MapMember( a => a.SecondLevel.ThirdLevel.A, b => b.A )
                    .MapMember( a => a.SecondLevel.GetThird().A, b => b.A1 )
                    .MapMember( a => a.GetSecond().GetThird().A, b => b.A2 )
                    .MapMember( a => a.A, b => b.SecondLevel.GetThird().A,
                        ( b, value ) => b.SecondLevel.GetThird().SetA( value ) );
            } );

            ultraMapper.Map( source, target );

            bool isResultOk = ultraMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void BuildGetterWithNullChecks()
        {
            Expression<Func<FirstLevel, string>> selector = t => t.SecondLevel.ThirdLevel.A;

            var accessPath = selector.GetMemberAccessPath();
            var expression = accessPath.GetGetterExpWithNullChecks();
            var functor = (Func<FirstLevel, string>)expression.Compile();

            // LEVEL 1
            var source = new FirstLevel();
            var result = functor( source );

            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );

            // LEVEL 2
            source = new FirstLevel()
            {
                SecondLevel = new SecondLevel()
            };

            result = functor( source );
            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );

            // LEVEL 3
            source = new FirstLevel()
            {
                SecondLevel = new SecondLevel()
                {
                    ThirdLevel = new ThirdLevel()
                }
            };

            result = functor( source );
            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );

            // LEVEL 4
            source = new FirstLevel()
            {
                SecondLevel = new SecondLevel()
                {
                    ThirdLevel = new ThirdLevel()
                    {
                        A = "Ok"
                    }
                }
            };

            result = functor( source );
            Assert.IsTrue( result == source?.SecondLevel?.ThirdLevel?.A );
        }
    }
}

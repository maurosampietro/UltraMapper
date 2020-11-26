using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
    [TestClass]
    public class MappingConventionTests
    {
        private class TestType
        {
            public List<int> List1 { get; set; }
            public List<int> List2 { get; set; }
        }

        private class GetMethodConventions
        {
            public List<int> GetList1()
            {
                return new List<int> { 0, 1, 2 };
            }

            public List<int> Get_List2()
            {
                return new List<int> { 3, 4, 5 };
            }
        }

        private class SetMethodConventions
        {
            private List<int> _list1;
            private List<int> _list2;

            public void SetList1( List<int> value ) { _list1 = value; }
            public void Set_List2( List<int> value ) { _list2 = value; }

            public List<int> GetList1() { return _list1; }
            public List<int> GetList2() { return _list2; }
        }

        [TestMethod]
        [Ignore]
        public void EnableConventionsGlobally()
        {

        }

        [TestMethod]
        [Ignore]
        public void EnableConventionsGloballyButDisableOnSpecificType()
        {

        }

        [TestMethod]
        [Ignore]
        public void DisableConventionsGlobally()
        {

        }

        [TestMethod]
        [Ignore]
        public void DisableConventionsGloballyButEnableOnSpecificType()
        {

        }

        [TestMethod]
        public void ConfigurationOptionOverride()
        {
            //In this test the collection update adds elements to the target.
            //This works if the capacity of the target list is updated BEFORE adding elements.

            var source = new TestType()
            {
                List1 = Enumerable.Range( 1, 10 ).ToList(),
                List2 = Enumerable.Range( 20, 10 ).ToList()
            };

            var target = new TestType()
            {
                List1 = Enumerable.Range( 30, 10 ).ToList(),
                List2 = Enumerable.Range( 40, 10 ).ToList()
            };

            var targetPrimitiveListCount = target.List1.Count;

            var mapper = new Mapper( cfg =>
            {
                cfg.CollectionBehavior = CollectionBehaviors.UPDATE;
                cfg.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;

                cfg.MapTypes<TestType, TestType>( typeConfig =>
                {
                    typeConfig.ReferenceBehavior = ReferenceBehaviors.CREATE_NEW_INSTANCE;
                    typeConfig.CollectionBehavior = CollectionBehaviors.RESET;
                } )
                .MapMember( s => s.List1, t => t.List1, memberConfig =>
                {
                    memberConfig.ReferenceBehavior = ReferenceBehaviors.USE_TARGET_INSTANCE_IF_NOT_NULL;
                    memberConfig.CollectionBehavior = CollectionBehaviors.MERGE;
                } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( target.List1.SequenceEqual( Enumerable.Range( 30, 10 ).Concat( Enumerable.Range( 1, 10 ) ) ) );
            Assert.IsTrue( source.List2.SequenceEqual( target.List2 ) );
        }

        [TestMethod]
        public void MethodMatching()
        {
            var source = new GetMethodConventions();
            var target = new SetMethodConventions();

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.SourceMemberProvider.IgnoreMethods = false;
                    convention.TargetMemberProvider.IgnoreMethods = false;

                    convention.MatchingRules.GetOrAdd<MethodMatching>();
                } );
            } );

            mapper.Map( source, target );

            Assert.IsTrue( source.GetList1().SequenceEqual( target.GetList1() ) );
            Assert.IsTrue( source.Get_List2().SequenceEqual( target.GetList2() ) );
        }

        private class A
        {
            public double Double { get; set; }
        }

        private class B
        {
            public float Double { get; set; }
        }

        [TestMethod]
        public void ExactNameAndExplicitConversionTypeMatching()
        {
            var source = new A() { Double = 11 };

            var mapper = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = false );
                } );
            } );

            var result = mapper.Map<B>( source );
            Assert.IsTrue( result.Double == 0 );

            var mapper2 = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = true );
                } );
            } );

            result = mapper2.Map<B>( source );
            Assert.IsTrue( result.Double == 11 );
        }

        private class C
        {
            public float Double { get; set; }
        }

        private class D
        {
            public double Double { get; set; }
        }

        [TestMethod]
        public void ExactNameAndConversionTypeMatching()
        {
            var source = new C() { Double = 11 };

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowImplicitConversions = false );
                } );
            } );

            var result = mapper.Map<D>( source );
            Assert.IsTrue( result.Double == 0 );

            var mapper2 = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<ExactNameMatching>()
                        .GetOrAdd<TypeMatchingRule>( ruleCfg => ruleCfg.AllowExplicitConversions = true );
                } );
            } );

            result = mapper2.Map<D>( source );
            Assert.IsTrue( result.Double == 11 );
        }

        private class E
        {
            public double? Double { get; set; }
        }

        private class F
        {
            public double Double { get; set; }
        }
    }

    /// <summary>
    /// Test simple scenarios, only public properties involved
    /// </summary>
    [TestClass]
    public class SimpleProjectionConventionTests
    {
        private class OrderDto
        {
            public string CustomerName { get; set; }
            public string ProductName { get; set; }
        }

        private class Order
        {
            public Customer Customer { get; set; }
            public Product Product { get; set; }
        }

        private class Product
        {
            public decimal Price { get; set; }
            public string Name { get; set; }
        }

        private class Customer
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void Flattening()
        {
            var customer = new Customer
            {
                Name = "George Costanza"
            };

            var product = new Product
            {
                Name = "Bosco",
                Price = 4.99m
            };

            var order = new Order
            {
                Customer = customer,
                Product = product
            };

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<ProjectionConvention>();
            } );

            var dto = mapper.Map<OrderDto>( order );

            Assert.IsTrue( dto.CustomerName == customer.Name );
            Assert.IsTrue( dto.ProductName == product.Name );
        }

        [TestMethod]
        public void Unflattening()
        {
            var dto = new OrderDto()
            {
                CustomerName = "Johnny",
                ProductName = "Mobile phone"
            };

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<ProjectionConvention>();
            } );

            //TODO: we need to create instances for nested objects!!!
            var order = mapper.Map<Order>( dto );

            Assert.IsTrue( dto.CustomerName == order.Customer.Name );
            Assert.IsTrue( dto.ProductName == order.Product.Name );
        }
    }

    [TestClass]
    ///Flattening taking methods and other member into account
    public class ComplexProjectionConventionTests
    {
        private class OrderDto
        {
            public string CustomerName { get; set; }

            public string _productName;
            public void SetProductName( string productName )
            {
                _productName = productName;
            }

            public string GetProductName()
            {
                return _productName;
            }
        }

        private class Order
        {
            public Customer Customer { get; set; }
            public Product Product { get; set; }
        }

        private class Product
        {
            public decimal Price { get; set; }
            public string Name { get; set; }
        }

        private class Customer
        {
            public string Name { get; set; }
        }

        [TestMethod]
        public void Flattening()
        {
            var customer = new Customer
            {
                Name = "George Costanza"
            };

            var product = new Product
            {
                Name = "Bosco",
                Price = 4.99m
            };

            var order = new Order
            {
                Customer = customer,
                Product = product
            };

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<ProjectionConvention>();
            } );

            var dto = mapper.Map<OrderDto>( order );

            Assert.IsTrue( dto.CustomerName == customer.Name );
            Assert.IsTrue( dto.GetProductName() == product.Name );
        }

        [TestMethod]
        public void Unflattening()
        {
            var dto = new OrderDto()
            {
                CustomerName = "Johnny",
            };

            dto.SetProductName( "Mobile phone" );

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<ProjectionConvention>();
            } );

            //TODO: we need to create instances for nested objects!!!
            var order = mapper.Map<Order>( dto );

            Assert.IsTrue( dto.CustomerName == order.Customer.Name );
            Assert.IsTrue( dto.GetProductName() == order.Product.Name );
        }
    }

    [TestClass]
    public class ProjectionNestedMethodCalls
    {
        private class OrderDto
        {
            private string _customerName;
            public string GetCustomerName() => _customerName;
            public void SetCustomerName( string name ) => _customerName = name;
        }

        private class Order
        {
            private Customer _customer;
            public Customer GetCustomer() => _customer;
            public void SetCustomer( Customer customer ) => _customer = customer;
        }

        private class Customer
        {
            private string _name;
            public void SetName( string name ) => _name = name;
            public string GetName() => _name;
        }

        [TestMethod]
        public void FlatteningNestedMethodCalls()
        {
            var customer = new Customer();
            customer.SetName( "George Costanza" );

            var order = new Order();
            order.SetCustomer( customer );

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<ProjectionConvention>();
            } );

            var dto = mapper.Map<OrderDto>( order );
            Assert.IsTrue( dto.GetCustomerName() == order.GetCustomer().GetName() );
        }

        [TestMethod]
        public void UnflatteningNestedMethodCalls()
        {
            var dto = new OrderDto();
            dto.SetCustomerName( "Johnny" );

            var mapper = new Mapper( cfg =>
            {
                cfg.Conventions.GetOrAdd<ProjectionConvention>();
            } );

            var order = mapper.Map<Order>( dto );
            Assert.IsTrue( dto.GetCustomerName() == order.GetCustomer().GetName() );
        }
    }

    [TestClass]
    public class MatchingRuleTests
    {
        public class TestClass
        {
            public int Property { get; set; }
        }

        public class PrefixTestClass
        {
            public int DtoProperty { get; set; }
        }

        public class SuffixTestClass
        {
            public int PropertyDto { get; set; }
        }

        [TestMethod]
        public void PrefixMatching()
        {
            //Property -> DtoProperty
            var source2 = new TestClass() { Property = 11 };
            var mapper2 = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<PrefixMatching>();
                } );
            } );

            var result2 = mapper2.Map<PrefixTestClass>( source2 );
            Assert.IsTrue( source2.Property == result2.DtoProperty );

            //DtoProperty -> Property
            var source = new PrefixTestClass() { DtoProperty = 11 };
            var mapper = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<PrefixMatching>();
                } );
            } );

            var result = mapper.Map<TestClass>( source );
            Assert.IsTrue( source.DtoProperty == result.Property );
        }

        [TestMethod]
        public void SuffixMatching()
        {
            //Property -> DtoProperty
            var source2 = new TestClass() { Property = 11 };
            var mapper2 = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<SuffixMatching>();
                } );
            } );

            var result2 = mapper2.Map<SuffixTestClass>( source2 );
            Assert.IsTrue( source2.Property == result2.PropertyDto );

            //DtoProperty -> Property
            var source = new SuffixTestClass() { PropertyDto = 11 };
            var mapper = new Mapper( cfg =>
            {
                var matching = new MatchingRules();

                cfg.Conventions.GetOrAdd<DefaultConvention>( convention =>
                {
                    convention.MatchingRules.GetOrAdd<SuffixMatching>();
                } );
            } );

            var result = mapper.Map<TestClass>( source );
            Assert.IsTrue( source.PropertyDto == result.Property );
        }
    }
}

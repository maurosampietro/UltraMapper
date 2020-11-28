using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
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
}

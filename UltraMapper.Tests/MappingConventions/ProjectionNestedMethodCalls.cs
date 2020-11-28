using Microsoft.VisualStudio.TestTools.UnitTesting;
using UltraMapper.Conventions;

namespace UltraMapper.Tests
{
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
}

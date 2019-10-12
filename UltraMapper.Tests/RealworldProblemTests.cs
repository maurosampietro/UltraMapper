using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UltraMapper.Tests
{
    [TestClass]
    public class RealworldBug1
    {
        public class Test
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public decimal Weight { get; set; }
            public DateTime Created { get; set; }
            public Guid ListId { get; set; }
            public int Type { get; set; }
            public Product Product { get; set; }
            public Product SpareProduct { get; set; }
            public List<Product> Products { get; set; }
        }

        public class Product
        {
            public Guid Id { get; set; }
            public string ProductName { get; set; }
            public decimal Weight { get; set; }
            public string Description { get; set; }
            public List<ProductVariant> Options { get; set; }
            public ProductVariant DefaultOption { get; set; }
        }

        public class ProductVariant
        {
            public Guid Id { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
        }

        public enum Types
        {
            Test = 1,
            Staging = 2,
            Production = 3
        }

        public class TestViewModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Age { get; set; }
            public decimal Weight { get; set; }
            public DateTime Created { get; set; }
            public Types Type { get; set; }
            public ProductViewModel Product { get; set; }
            public ProductViewModel SpareTheProduct { get; set; }
            public List<ProductViewModel> Products { get; set; }
        }

        public class ProductViewModel
        {
            public Guid Id { get; set; }
            public string ProductName { get; set; }
            public decimal Weight { get; set; }
            public string Description { get; set; }
            public List<ProductVariantViewModel> Options { get; set; }
            public ProductVariantViewModel DefaultSharedOption { get; set; }
        }

        public class ProductVariantViewModel
        {
            public Guid Id { get; set; }
            public string Color { get; set; }
            public string Size { get; set; }
        }

        public static class DataGenerator
        {
            public static List<Test> GetTests( int count )
            {

                var colors = new List<string>( Enum.GetNames( typeof( ConsoleColor ) ) );

                var result = new List<Test>();
                var random = new Random();

                var productList = new List<Product>();

                var productOptionList = new List<ProductVariant>();

                for( var j = 0; j < random.Next( 1, 15 ); j++ )
                {
                    productOptionList.Add(
                        new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            Color = colors[ random.Next( 0, colors.Count - 1 ) ],
                            Size = string.Format( "Universal - {0}", j )
                        }
                        );
                }

                for( var i = 0; i < random.Next( 1, 20 ); i++ )
                {
                    var productVariantList = new List<ProductVariant>();
                    for( var j = 0; j < random.Next( 1, 5 ); j++ )
                    {
                        productVariantList.Add(
                            new ProductVariant
                            {
                                Id = Guid.NewGuid(),
                                Color = colors[ random.Next( 0, colors.Count - 1 ) ],
                                Size = string.Format( "Universal - {0} - {1}", i, j )
                            }
                            );
                    }

                    productList.Add( new Product
                    {
                        Id = Guid.NewGuid(),
                        ProductName = "PRODUCT in COLLECTION" + i,
                        Description = "PRODUCT in COLLECTION description" + i,
                        Weight = Convert.ToDecimal( Math.Round( random.NextDouble() * 100, 2 ) ),
                        DefaultOption = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            Color = colors[ random.Next( 0, colors.Count - 1 ) ],
                            Size = i.ToString( CultureInfo.InvariantCulture )
                        },
                        Options = productVariantList
                    } );
                }

                for( var i = 0; i < count; i++ )
                {
                    var test = new Test
                    {
                        Id = Guid.NewGuid(),
                        Age = i % 10,
                        Created = DateTime.Now,
                        Name = "Test" + i,
                        Weight = random.Next( 5, 99999 ),
                        Type = random.Next( 1, 3 ),
                        Product = new Product
                        {
                            Id = Guid.NewGuid(),
                            ProductName = "TEST PRODUCT" + i,
                            Description = "TEST PRODUCT description" + i,
                            Weight = Convert.ToDecimal( Math.Round( random.NextDouble() * 100, 2 ) ),
                            Options = productOptionList,
                            DefaultOption = new ProductVariant
                            {
                                Id = Guid.NewGuid(),
                                Color = colors[ random.Next( 0, colors.Count - 1 ) ],
                                Size = "Matt"
                            }
                        },
                        SpareProduct = new Product
                        {
                            Id = Guid.NewGuid(),
                            ProductName = "SPARE TEST PRODUCT" + i,
                            Description = "SPARE TEST PRODUCT description" + i,
                            Weight = Convert.ToDecimal( Math.Round( random.NextDouble() * 100, 2 ) ),
                            Options = new List<ProductVariant>
                                {
                                    new ProductVariant
                                    {
                                        Id = Guid.NewGuid(),
                                        Color = colors[random.Next(0, colors.Count - 1)],
                                        Size = "Universal"
                                    }
                                },
                            DefaultOption = new ProductVariant
                            {
                                Id = Guid.NewGuid(),
                                Color = colors[ random.Next( 0, colors.Count - 1 ) ],
                                Size = "Matt"
                            }
                        },
                        Products = productList
                    };
                    result.Add( test );
                }
                return result;
            }
        }

        [TestMethod]
        public void TestProblem()
        {
            var list = DataGenerator.GetTests( 1 );

            var mapper = new Mapper();
            var result = mapper.Map<List<TestViewModel>>( list );

            bool isResultOk = mapper.VerifyMapperResult( list, result );
            Assert.IsTrue( isResultOk );
        }
    }

    [TestClass]
    public class RealworldBug2
    {
        public class SourceClass
        {
            public string Id { get; set; }
        }

        public class TargetClass
        {
            public Guid Id { get; set; }
        }

        [TestMethod]
        public void StringToGuid()
        {
            var mapper = new Mapper( cfg =>
            {
                cfg.MapTypes<string, Guid>( str => Guid.Parse( str ) );
            } );


            var source = new SourceClass() { Id = Guid.NewGuid().ToString() };
            var result = mapper.Map<TargetClass>( source );

            bool isResultOk = mapper.VerifyMapperResult( source, result );
            Assert.IsTrue( isResultOk );
        }
    }
}

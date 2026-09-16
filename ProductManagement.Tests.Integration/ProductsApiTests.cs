using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Server;
using NUnit.Framework;
using ProductManagement.Api.Entities;
using ProductManagement.Api.Data;

namespace ProductManagement.Tests.Integration
{
    [TestFixture]
    internal class ProductsApiTests : ProductApiTestBase
    {
        [TearDown]
        public async Task LocalTearDown()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Products.RemoveRange(context.Products);
            await context.SaveChangesAsync();
        }

        [SetUp]
        public void localSetUp()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "Auth_Token");
        }

        [Test]
        public async Task CreateProduct_Test()
        {
            

            var testPayload = new Product()
            {
                Name = "Test",
                Sku = "test_Sku",
                Price = 200.00m,
                StockQuantity = 100
            };

            var postResponse = await _client.PostAsJsonAsync("api/Products", testPayload);

            postResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }



        [Test]
        public async Task GetProducts_Test()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Products.Add(new Product()
            {
                Name = "Test",
                Sku = "test_Sku",
                Price = 200.00m,
                StockQuantity = 100
            });
            await context.SaveChangesAsync();

            var getResponse = await _client.GetAsync("api/Products");
            getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var products = await getResponse.Content.ReadFromJsonAsync<IEnumerable<Product>>();
            products.Should().NotBeNull();
            products!.Count().Should().Be(1);

        }
        [Test]
        public async Task GetLowStockProducts_Test()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Products.AddRange(new Product()
            {
                Name = "Test1",
                Sku = "test_Sku1",
                Price = 200.00m,
                StockQuantity = 100
            },
            new Product()
            {
                Name = "Test2",
                Sku = "test_Sku2",
                Price = 200.00m,
                StockQuantity = 50
            },
            new Product()
            {
                Name = "Test3",
                Sku = "test_Sku3",
                Price = 200.00m,
                StockQuantity = 25
            });
            await context.SaveChangesAsync();

            var response = await _client.GetAsync("api/Products/low-stock?threshhold=30");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var products = await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();

            products.Should().NotBeNull();
            products!.Count().Should().Be(1);
            products!.First().Name.Should().Be("Test3");
            products!.First().Sku.Should().Be("test_Sku3");

        }
    }
}

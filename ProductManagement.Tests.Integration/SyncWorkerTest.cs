using NUnit.Framework;
using ProductManagement.Api.Data;
using ProductManagement.Api.Entities;
using ProductManagement.Api.Enums;
using Moq;
using ProductManagement.Api.BackgroundServices;
using FluentAssertions;



namespace ProductManagement.Tests.Integration
{
    [TestFixture]
    public class SyncWorkerTest : ProductApiTestBase
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
        public async Task SyncProducts_Test()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var testProduct = new Product()
                {
                    Name = "Test_Worker",
                    Sku = "test_Sku",
                    Price = 200.00m,
                    StockQuantity = 100,
                    SyncStatus = SyncStatus.Pending
                };
                context.Products.Add(testProduct);
                await context.SaveChangesAsync();

            }

            var mockLoger = new Mock<ILogger<SyncWorker>>();

            var worker = new SyncWorker(_factory.Services.GetService<IServiceScopeFactory>(), mockLoger.Object);

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await worker.SyncProductsAsync(CancellationToken.None);

            }

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var processedProduct = context.Products.Single(p => p.Name == "Test_Worker");
                processedProduct.SyncStatus.Should().Be(SyncStatus.Synchronized);
            }
        }

    }
}

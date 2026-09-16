using EntityFrameworkCore.Testing.Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ProductManagement.Api.BuisnessLogic;
using ProductManagement.Api.Data;
using ProductManagement.Api.Entities;
using ProductManagement.Api.Enums;


namespace ProductManagement.Tests.Unit
{
    [TestFixture]
    internal class ProductServiceTests
    {
        
        private AppDbContext _mockDbContext;
        private Mock<ILogger<ProductService>> _logger;
        private ProductService _service;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = Create.MockedDbContextFor<AppDbContext>();
            _logger = new Mock<ILogger<ProductService>>();

            _service = new ProductService(_mockDbContext, _logger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockDbContext.Dispose();
        }

        [Test]
        public async Task ProcessAndAddProductAsyc_ShouldSanitizeAndUpperCaseSku()
        {
            var product = new Product()
            {
                Sku = "  erp_prod_125  ",
                CreatedAt = DateTime.UtcNow,
                Name = "Test",
                Price = 200.00m,
                StockQuantity = 40
            };

            var result = await _service.ProcessAndAddProductAsyc(product);

            result.Sku.Should().Be("ERP-PROD-125");
            result.SyncStatus.Should().Be(SyncStatus.Pending);
        }

        [TestCase(10)]
        [TestCase(3)]
        public async Task GetProcessedProductsAsync_ShouldApplyProcedureWhenSockIsBellow_Treshold(int stockQuantity)
        {
            var product = new Product()
            {
                Sku = "ERP-PROD-125",
                CreatedAt = DateTime.UtcNow,
                Name = "Test",
                Price = 100.00m,
                StockQuantity = stockQuantity
            };

            _mockDbContext.Products.Add(product);
            await _mockDbContext.SaveChangesAsync();

            var result = await _service.GetProcessedProductsAsync();

            var testProduct = result.First();

            testProduct.Price.Should().Be(150.00m);


        }

    }
}

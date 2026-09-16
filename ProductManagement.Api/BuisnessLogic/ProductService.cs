using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.Data;
using ProductManagement.Api.Entities;
using ProductManagement.Api.Enums;
using System.Text.RegularExpressions;

namespace ProductManagement.Api.BuisnessLogic
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<ProductService> _logger;
        private const int CriticalThreshold = 20;

        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            this._appDbContext = context;
            this._logger = logger;

        }
        public async Task<IEnumerable<Product>> GetProcessedProductsAsync()
        {
            var products = await _appDbContext.Products.ToListAsync();
            foreach (var product in products)
            {
                if(product.StockQuantity <= CriticalThreshold)
                {
                    _logger.LogWarning("Product {Sku} is bellow stock threshold. Current Stock {Stock}.", product.Sku, product.StockQuantity);

                    product.Price = Math.Round(product.Price * 1.5m, 2); //??

                }
            }

            return products;
        }

        public async Task<Product> ProcessAndAddProductAsyc(Product product)
        {
            if (String.IsNullOrWhiteSpace(product.Sku))
            {
                throw new ArgumentNullException("Issue with Sku. Please verify it.");
            }

            product.Sku = Regex.Replace(product.Sku.Trim().ToUpper(), @"[\s_]+", "-");

            var exist = await _appDbContext.Products.AnyAsync(x => x.Sku == product.Sku);

            if (exist)
            {
                throw new InvalidOperationException("Sku already exist in the database");
            }

            product.CreatedAt = DateTime.UtcNow;
            product.SyncStatus = SyncStatus.Pending;
            _appDbContext.Products.Add(product);
            await _appDbContext.SaveChangesAsync();

            return product;

        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        {
            var result = await _appDbContext.Products.Where(x => x.StockQuantity <= threshold).OrderBy(p => p.StockQuantity).ToListAsync();

            return result;
        }
    }
}

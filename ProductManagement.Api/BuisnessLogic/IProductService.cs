using ProductManagement.Api.Entities;

namespace ProductManagement.Api.BuisnessLogic
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProcessedProductsAsync();

        Task<Product> ProcessAndAddProductAsyc(Product product);

        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);

    }
}

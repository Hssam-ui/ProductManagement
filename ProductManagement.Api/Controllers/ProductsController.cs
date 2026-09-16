using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Api.BuisnessLogic;
using ProductManagement.Api.Data;
using ProductManagement.Api.Entities;
using System.Diagnostics;


namespace ProductManagement.Api.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "UserPolicy")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        { 
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var result = await _productService.GetProcessedProductsAsync();
            return Ok(result);
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProduct([FromQuery] int threshhold = 5)
        {
            var result = await _productService.GetLowStockProductsAsync(threshhold);

            return Ok(result);
            
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (product == null)
            {
                return BadRequest("Product cannot be null.");
            }

            var result = await _productService.ProcessAndAddProductAsyc(product);
            return Created("",result);
        }
    }
}

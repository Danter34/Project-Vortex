using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository productRepository, ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        [HttpGet("get-all-product")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn,
                                         [FromQuery] string? filterQuery,
                                         [FromQuery] string? sortBy,
                                         [FromQuery] bool isAscending = true,
                                         [FromQuery] int pageNumber = 1,
                                         [FromQuery] int pageSize = 100)
        {
            _logger.LogInformation(
                "Fetching all products. Page: {PageNumber}, PageSize: {PageSize}, FilterOn: {FilterOn}, FilterQuery: {FilterQuery}, SortBy: {SortBy}, Ascending: {IsAscending}",
                pageNumber, pageSize, filterOn, filterQuery, sortBy, isAscending
            );

            var products = await _productRepository.GetAllProduct(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            if (products == null || !products.Any())
            {
                _logger.LogWarning("No products found with the given parameters.");
                return NotFound(new { message = "Không tìm thấy sản phẩm nào phù hợp." });
            }

            //  Gắn full URL cho ảnh sản phẩm
            foreach (var product in products)
            {
                if (product.Images != null && product.Images.Any())
                {
                    foreach (var img in product.Images)
                    {
                        if (!string.IsNullOrEmpty(img.FilePath) && !img.FilePath.StartsWith("http"))
                        {
                            img.FilePath = $"{Request.Scheme}://{Request.Host}{img.FilePath}";
                        }
                    }
                }
            }

            _logger.LogInformation("{Count} products retrieved successfully.", products.Count());
            return Ok(products);
        }


        [HttpGet("get-product-by-id/{id}")]
        // [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching product by ID: {Id}", id);

            var product = await _productRepository.GetProductById(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return NotFound(new { message = $"Không tìm thấy sản phẩm với ID = {id}" });
            }

            //  Gắn full URL cho ảnh sản phẩm (giống GetAll)
            if (product.Images != null && product.Images.Any())
            {
                foreach (var img in product.Images)
                {
                    if (!string.IsNullOrEmpty(img.FilePath) && !img.FilePath.StartsWith("http"))
                    {
                        img.FilePath = $"{Request.Scheme}://{Request.Host}{img.FilePath}";
                    }
                }
            }

            _logger.LogInformation("Product with ID {Id} retrieved successfully.", id);
            return Ok(product);
        }


        [HttpPost("add-product")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductDTO dto)
        {
            _logger.LogInformation("Creating a new product: {ProductName}", dto.Title);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product data submitted.");
                return BadRequest(ModelState);
            }

            var newProduct = await _productRepository.CreateProduct(dto);

            //  build URL tuyệt đối cho từng ảnh
            if (newProduct.Images != null && newProduct.Images.Any())
            {
                foreach (var img in newProduct.Images)
                {
                    // Nếu FilePath chưa phải URL tuyệt đối
                    if (!img.FilePath.StartsWith("http"))
                    {
                        img.FilePath = $"{Request.Scheme}://{Request.Host}{img.FilePath}";
                    }
                }
            }

            _logger.LogInformation("Product created successfully with ID: {Id}", newProduct.Id);

            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
        }


        [HttpPut("update-product-by-id/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] EdditProductDTO dto)
        {
            _logger.LogInformation("Updating product ID {Id}", id);

            var updated = await _productRepository.UpdateProduct(id, dto);
            if (updated == null)
            {
                _logger.LogWarning("Product ID {Id} not found for update.", id);
                return NotFound();
            }

            _logger.LogInformation("Product ID {Id} updated successfully.", id);
            return Ok(updated);
        }

        [HttpDelete("delete-product/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting product ID {Id}", id);

            var deleted = await _productRepository.DeleteProduct(id);
            if (!deleted)
            {
                _logger.LogWarning("Product ID {Id} not found for deletion.", id);
                return NotFound();
            }

            _logger.LogInformation("Product ID {Id} deleted successfully.", id);
            return NoContent();
        }
    }
}

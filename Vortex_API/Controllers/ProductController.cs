using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet("get-all-product")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn,
        [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var products = await _productRepository.GetAllProduct(filterOn,
            filterQuery,
            sortBy,
            isAscending,
            pageNumber,
            pageSize);
            if (products == null || !products.Any())
                return NotFound(new { message = "Không tìm thấy sản phẩm nào phù hợp." });
            return Ok(products);
        }

        [HttpGet("get-product-by-id/{id}")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost("add-product")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newProduct = await _productRepository.CreateProduct(dto);
            return CreatedAtAction(nameof(GetById), new { id = newProduct.Id }, newProduct);
        }

        [HttpPut("update-product-by-id/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] EdditProductDTO dto)
        {
            var updated = await _productRepository.UpdateProduct(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("delete-product/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productRepository.DeleteProduct(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

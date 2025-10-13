using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
using Vortex_API.Repositories.Service;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryController(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        [HttpGet("get-all-category")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? filterOn,
        [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var categories = await _categoryRepo.GetAllCategory(
            filterOn,
            filterQuery,
            sortBy,
            isAscending,
            pageNumber,
            pageSize);
            if (categories == null || !categories.Any())
                return NotFound(new { message = "Không tìm thấy danh mục nào phù hợp." });
            return Ok(categories);
        }

        [HttpGet("get-category-by-id/{id}")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepo.GetCategoryById(id);
            if (category == null)
                return NotFound(new { message = "Category not found" });

            return Ok(category);
        }

        [HttpPost("add-category")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CategoryDTO categoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _categoryRepo.CreateCategory(categoryDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("update-category-by-id/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDTO categoryDto)
        {
            var updated = await _categoryRepo.UpdateCategory(id, categoryDto);
            if (updated == null)
                return NotFound(new { message = "Category not found" });

            return Ok(updated);
        }

        [HttpDelete("delete-category/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _categoryRepo.DeleteCategory(id);
            if (!success)
                return NotFound(new { message = "Category not found" });

            return NoContent();
        }
    }
}

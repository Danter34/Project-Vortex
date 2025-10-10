using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly INewsRepository _newsRepository;

        public NewsController(INewsRepository newsRepository)
        {
            _newsRepository = newsRepository;
        }

        [HttpGet("get-all-news")]
        public async Task<IActionResult> GetAll()
        {
            var newsList = await _newsRepository.GetAllNews();
            return Ok(newsList);
        }

        [HttpGet("get-banner-by-id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var news = await _newsRepository.GetNewsById(id);
            if (news == null)
                return NotFound();

            return Ok(news);
        }

        [HttpPost("add-news")]
        public async Task<IActionResult> Create([FromForm] NewsDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _newsRepository.CreateNews(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("update-by-id/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EdditNewsDTO dto)
        {
            var updated = await _newsRepository.UpdateNewsbyId(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("delete-news/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _newsRepository.DeleteNews(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

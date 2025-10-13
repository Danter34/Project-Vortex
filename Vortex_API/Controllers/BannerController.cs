using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class BannerController : ControllerBase
    {
        private readonly IBannerRepository _bannerRepository;

        public BannerController(IBannerRepository bannerRepository)
        {
            _bannerRepository = bannerRepository;
        }

        [HttpGet("Get-all-banner")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetAll()
        {
            var banners = await _bannerRepository.GetAllBanner();
            return Ok(banners);
        }

        [HttpGet("get-banner-by-id/{id}")]
        //[Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var banner = await _bannerRepository.GetBannerById(id);
            if (banner == null)
                return NotFound();
            return Ok(banner);
        }

        [HttpPost("add-banner")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] BannerDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newBanner = await _bannerRepository.CreateBanner(dto);
            return CreatedAtAction(nameof(GetById), new { id = newBanner.Id }, newBanner);
        }

        [HttpPut("update-banner-by-id/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] EdditBannerDTO dto)
        {
            var updated = await _bannerRepository.UpdateBanner(id, dto);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("delete-banner-by-id/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _bannerRepository.DeleteBanner(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}

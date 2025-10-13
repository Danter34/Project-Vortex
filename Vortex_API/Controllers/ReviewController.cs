using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet("get-review{productId}")]
        public async Task<IActionResult> GetReviews(int productId)
        {
            var reviews = await _reviewRepository.GetReviewsByProduct(productId);
            return Ok(reviews);
        }

        [Authorize]
        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var review = await _reviewRepository.AddReview(userId!, dto);
                return Ok(review);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

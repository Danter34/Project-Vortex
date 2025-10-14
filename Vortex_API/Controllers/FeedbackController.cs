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
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackController(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await _feedbackRepository.CreateFeedback(userId, dto);
            if (result == null)
                return BadRequest("Bạn chỉ có thể gửi 1 phản hồi trong vòng 24 giờ.");

            return Ok(result);
        }

        [HttpGet("my-feedback")]
        [Authorize]
        public async Task<IActionResult> GetMyFeedback()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var feedback = await _feedbackRepository.GetFeedbackByUser(userId);
            if (feedback == null) return NotFound("Không có phản hồi nào.");

            return Ok(feedback);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _feedbackRepository.GetAllFeedbacks();
            return Ok(feedbacks);
        }

        [HttpPost("reply/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyFeedback(int id, [FromBody] string reply)
        {
            var success = await _feedbackRepository.ReplyFeedback(id, reply);
            if (!success) return NotFound("Không tìm thấy phản hồi.");
            return Ok("Đã phản hồi thành công.");
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var success = await _feedbackRepository.DeleteFeedback(id);
            if (!success) return NotFound("Không tìm thấy phản hồi.");
            return Ok("Đã xóa phản hồi.");
        }
    }
}

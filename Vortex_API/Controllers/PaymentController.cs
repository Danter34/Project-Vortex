using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vortex_API.Repositories.Interface;
using Vortex_API.Model.DTO;
using Microsoft.AspNetCore.Authorization;
namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        [HttpPost("create-momo")]
        public async Task<IActionResult> CreateMomoPayment([FromBody] PaymentRequest dto)
        {
            var result = await _paymentRepository.CreatePayment(dto);
            return Ok(result);
        }

        [HttpGet("momo-return")]
        public async Task<IActionResult> MomoReturn([FromQuery] string orderId, [FromQuery] string resultCode)
        {
            var success = await _paymentRepository.HandleMomoReturn(orderId, resultCode);
            return success ? Ok("Payment successful!") : BadRequest("Payment failed!");
        }

        [HttpPost("momo-notify")]
        public async Task<IActionResult> MomoNotify([FromForm] string orderId, [FromForm] string resultCode)
        {
            var success = await _paymentRepository.HandleMomoNotify(orderId, resultCode);
            return success ? Ok() : BadRequest();
        }
    }
}

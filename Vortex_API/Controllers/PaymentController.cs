using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vortex_API.Repositories.Interface;
using Vortex_API.Model.DTO;
using Microsoft.AspNetCore.Authorization;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentRepository paymentRepository, ILogger<PaymentController> logger)
        {
            _paymentRepository = paymentRepository;
            _logger = logger;
        }

        [HttpPost("create-momo")]
        public async Task<IActionResult> CreateMomoPayment([FromBody] PaymentRequest dto)
        {
            _logger.LogInformation("Creating MoMo payment for OrderId: {OrderId}", dto.OrderId);

            var result = await _paymentRepository.CreatePayment(dto);

            _logger.LogInformation("MoMo payment creation result for OrderId {OrderId}: {Result}", dto.OrderId, result);

            return Ok(result);
        }

        [HttpGet("momo-return")]
        public async Task<IActionResult> MomoReturn([FromQuery] string orderId, [FromQuery] string resultCode)
        {
            _logger.LogInformation("MoMo return called for OrderId: {OrderId}, ResultCode: {ResultCode}", orderId, resultCode);

            var success = await _paymentRepository.HandleMomoReturn(orderId, resultCode);

            if (success)
            {
                _logger.LogInformation("Payment successful for OrderId: {OrderId}", orderId);
                return Ok("Payment successful!");
            }
            else
            {
                _logger.LogWarning("Payment failed for OrderId: {OrderId}", orderId);
                return BadRequest("Payment failed!");
            }
        }

        [HttpPost("momo-notify")]
        public async Task<IActionResult> MomoNotify([FromForm] string orderId, [FromForm] string resultCode)
        {
            _logger.LogInformation("MoMo notify received for OrderId: {OrderId}, ResultCode: {ResultCode}", orderId, resultCode);

            var success = await _paymentRepository.HandleMomoNotify(orderId, resultCode);

            if (success)
            {
                _logger.LogInformation("MoMo notify processed successfully for OrderId: {OrderId}", orderId);
                return Ok();
            }
            else
            {
                _logger.LogWarning("MoMo notify failed for OrderId: {OrderId}", orderId);
                return BadRequest();
            }
        }
    }
}

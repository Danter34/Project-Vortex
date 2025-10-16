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
            try
            {
                var success = await _paymentRepository.HandleMomoReturn(orderId, resultCode);

                // Nếu đang chạy local (localhost:7080 hoặc 7161) thì redirect sang MVC view
                var isLocal = Request.Host.Host.Contains("localhost");

                if (isLocal)
                {
                    if (success)
                        return Redirect($"https://localhost:7080/Checkout/Success?orderId={orderId}");
                    else
                        return Redirect($"https://localhost:7080/Checkout/Failed?orderId={orderId}");
                }

                // Nếu không phải local, trả về JSON
                return success ? Ok("Payment successful!") : BadRequest("Payment failed!");
            }
            catch (Exception ex)
            {
                // Trường hợp lỗi trừ kho / hết hàng
                _logger.LogError(ex, "Error handling MoMo return.");
                if (Request.Host.Host.Contains("localhost"))
                    return Redirect($"https://localhost:7080/Checkout/Failed?orderId={orderId}");
                return BadRequest(new { message = ex.Message });
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

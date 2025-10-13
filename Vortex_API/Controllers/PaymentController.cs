using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
using QRCoder;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentRepository _paymentRepo;

        public PaymentController(IPaymentService paymentService, IPaymentRepository paymentRepo)
        {
            _paymentService = paymentService;
            _paymentRepo = paymentRepo;
        }

        // Tạo payment và sinh QR 
        [Authorize]
        [HttpPost("create-online")]
        public async Task<IActionResult> CreateOnline([FromBody] PaymentRequestDTO dto)
        {
            var payment = await _paymentService.CreatePaymentAsync(dto);
            var scheme = Request.Scheme;
            var host = Request.Host.Value;
            var paymentPageUrl = $"{scheme}://{host}/pay/{payment.Id}";

            // tạo QR
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(paymentPageUrl, QRCodeGenerator.ECCLevel.Q);
            using var png = new PngByteQRCode(qrData);
            var pngBytes = png.GetGraphic(20);
            var base64 = Convert.ToBase64String(pngBytes);

            payment.QrDataUrl = $"data:image/png;base64,{base64}";
            await _paymentRepo.UpdateAsync(payment);

            var resp = new PaymentResponseDTO
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Method = payment.Method,
                Status = payment.Status,
                QrDataUrl = payment.QrDataUrl,
                PaymentPageUrl = paymentPageUrl
            };

            return Ok(resp);
        }

        // Trang hiển thị thông tin thanh toán
        [HttpGet("pay/{paymentId}")]
        public async Task<IActionResult> PaymentPage(int paymentId)
        {
            var payment = await _paymentService.GetByIdAsync(paymentId);
            if (payment == null)
                return NotFound("Payment not found");

            var html = $@"
<!doctype html>
<html lang='vi'>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<title>Thanh toán đơn hàng #{payment.OrderId}</title>
<style>
body {{
  font-family: 'Segoe UI', Arial, sans-serif;
  background: #f8f9fa;
  color: #333;
  margin: 0;
  padding: 20px;
}}
.container {{
  max-width: 600px;
  margin: 0 auto;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  padding: 24px;
}}
h2 {{ text-align: center; margin-bottom: 20px; }}
.field {{ margin-bottom: 15px; }}
label {{ font-weight: 600; display:block; margin-bottom: 6px; }}
input[readonly], textarea[readonly] {{
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 6px;
  background: #f7f7f7;
}}
.buttons {{
  display: flex;
  gap: 10px;
  justify-content: center;
  margin-top: 20px;
}}
button {{
  padding: 10px 18px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 500;
}}
button.copy {{ background: #0d6efd; color: #fff; }}
button.simulate {{ background: #6c757d; color: #fff; }}
.note {{
  font-size: 0.9rem;
  color: #555;
  margin-top: 12px;
  text-align: center;
}}
.qr {{
  text-align: center;
  margin-bottom: 20px;
}}
.qr img {{
  width: 180px;
  height: 180px;
}}
</style>
</head>
<body>
<div class='container'>
  <h2>Thanh toán đơn hàng #{payment.OrderId}</h2>

  <div class='qr'>
    <img src='{payment.QrDataUrl}' alt='QR Code'>
    <p>Quét mã để mở trang này</p>
  </div>

  <div class='field'>
    <label>Ngân hàng</label>
    <input readonly value='{HtmlEncoder.Default.Encode(payment.BankName)}'>
  </div>
  <div class='field'>
    <label>Số tài khoản</label>
    <input readonly value='{HtmlEncoder.Default.Encode(payment.ReceiverAccount)}'>
  </div>
  <div class='field'>
    <label>Người nhận</label>
    <input readonly value='{HtmlEncoder.Default.Encode(payment.ReceiverName)}'>
  </div>
  <div class='field'>
    <label>Số tiền (VND)</label>
    <input id='amountField' readonly value='{payment.Amount}'>
  </div>
  <div class='field'>
    <label>Nội dung</label>
    <textarea readonly rows='2'>{HtmlEncoder.Default.Encode(payment.Memo)}</textarea>
  </div>

  <div class='buttons'>
    <button class='copy' onclick='copyAll()'> Sao chép thông tin</button>
    <button class='simulate' onclick='markPaid()'> Đã chuyển / Giả lập thanh toán</button>
  </div>

  <p class='note'>*Số tiền và nội dung đã được tự động điền và không thể chỉnh sửa.</p>
</div>

<script>
document.getElementById('amountField').value = Number({payment.Amount}).toLocaleString('vi-VN');

function copyAll() {{
  const text = [
    'Ngân hàng: {HtmlEncoder.Default.Encode(payment.BankName)}',
    'Số tài khoản: {HtmlEncoder.Default.Encode(payment.ReceiverAccount)}',
    'Người nhận: {HtmlEncoder.Default.Encode(payment.ReceiverName)}',
    'Số tiền: {payment.Amount}',
    'Nội dung: {HtmlEncoder.Default.Encode(payment.Memo)}'
  ].join('\\n');
  navigator.clipboard.writeText(text).then(() => alert(' Đã sao chép thông tin!'));
}}

function markPaid() {{
  if (!confirm('Xác nhận giả lập thanh toán?')) return;
  fetch('/api/payment/{payment.Id}/confirm', {{ method: 'POST' }})
    .then(r => r.json())
    .then(j => {{
      alert(j.message || 'Đã đánh dấu thanh toán!');
      location.reload();
    }})
    .catch(e => alert('Lỗi: ' + e));
}}
</script>
</body>
</html>";

            return Content(html, "text/html");
        }

        //  Xác nhận thanh toán 
        [HttpPost("Confirm{paymentId}")]
        public async Task<IActionResult> Confirm(int paymentId)
        {
            try
            {
                await _paymentService.ConfirmPaymentAsync(paymentId);
                return Ok(new { message = "Payment confirmed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

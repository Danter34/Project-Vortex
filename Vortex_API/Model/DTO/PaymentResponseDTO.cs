namespace Vortex_API.Model.DTO
{
    public class PaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public string? QrDataUrl { get; set; } // data:image/png;base64,
        public string? PaymentPageUrl { get; set; } // the url that QR points to
    }
}

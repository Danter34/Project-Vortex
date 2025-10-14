namespace Vortex_API.Model.DTO
{
    public class PaymentResponse
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string PayUrl { get; set; }
        public string Status { get; set; }
    }
}

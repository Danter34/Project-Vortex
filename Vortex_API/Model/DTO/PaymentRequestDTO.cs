namespace Vortex_API.Model.DTO
{
    public class PaymentRequestDTO
    {
        public int OrderId { get; set; }
        public string Method { get; set; } = "ONLINE_QR";

        public string BankName { get; set; } = "Vietinbank";
        public string ReceiverAccount { get; set; } = "0123456789";
        public string ReceiverName { get; set; } = "Dante";
        public string? Memo { get; set; }
    }
}

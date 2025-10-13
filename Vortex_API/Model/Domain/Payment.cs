using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vortex_API.Model.Domain
{
    public class Payment
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(100)]
        public string Method { get; set; } = "ONLINE_QR";

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";

        // fields to show in the pay page
        public string BankName { get; set; } = "Vietinbank";
        public string ReceiverAccount { get; set; } = "0123456789";
        public string ReceiverName { get; set; } = "Dante";
        public string Memo { get; set; }

        // The generated QR image as data url
        public string? QrDataUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

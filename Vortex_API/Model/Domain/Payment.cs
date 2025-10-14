using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vortex_API.Model.Domain
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "MoMo";
        public string Status { get; set; } = "Pending";
        public string? PayUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string MomoOrderId { get; set; } 
    }
}

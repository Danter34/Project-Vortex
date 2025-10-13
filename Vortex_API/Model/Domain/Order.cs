using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.Domain
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        [MaxLength(500)]
        public string? ShippingAddress { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string Status { get; set; } = "Pending";
    }
}

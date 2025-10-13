using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.Domain
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}

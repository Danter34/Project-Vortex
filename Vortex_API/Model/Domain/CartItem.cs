using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.Domain
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required]
        public int CartId { get; set; }

        public Cart? Cart { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}

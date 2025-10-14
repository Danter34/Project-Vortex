using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.Domain
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public double AverageRating { get; set; } = 0;
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }

        // Liên kết tới danh mục
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        // Danh sách ảnh của sản phẩm
        public ICollection<Image>? Images { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int StockQuantity { get; set; } 
    }
}

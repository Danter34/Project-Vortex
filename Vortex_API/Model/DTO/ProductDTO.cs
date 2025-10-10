using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.DTO
{
    public class ProductDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public double Rate { get; set; } = 0;
        public bool IsHot { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
    public class EdditProductDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public double Rate { get; set; } = 0;
        public bool IsHot { get; set; }

        [Required]
        public int CategoryId { get; set; }

        //public List<IFormFile>? Images { get; set; }
    }
}

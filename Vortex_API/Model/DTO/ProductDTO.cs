using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.DTO
{
    public class ProductDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool IsHot { get; set; }
        public bool IsNew { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
    public class EdditProductDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }
        public int StockQuantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool IsHot { get; set; }
        public bool IsNew { get; set; }

        [Required]
        public int CategoryId { get; set; }

        //public List<IFormFile>? Images { get; set; }
    }
}

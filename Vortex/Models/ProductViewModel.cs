using Newtonsoft.Json;

namespace Vortex.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
        public int CategoryId { get; set; }
        public CategoryViewModel Category { get; set; }
        public string? CategoryName { get; set; }
        public List<ImageViewModel> Images { get; set; } = new();
        public double AverageRating { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public int StockQuantity { get; set; }
    }
    public class ProductCreateModel
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }

        public int CategoryId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }


    public class ProductEditModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public bool IsHot { get; set; }
        public bool IsNew { get; set; }
    }


    public class ImageViewModel
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
    }
}

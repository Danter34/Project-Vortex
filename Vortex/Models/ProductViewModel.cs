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
        public string CategoryName { get; set; }
        public List<ImageViewModel> Images { get; set; } = new();
        public double AverageRating { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new();
        public int StockQuantity { get; set; }
    }

    public class ImageViewModel
    {
        public string FilePath { get; set; }
    }
}

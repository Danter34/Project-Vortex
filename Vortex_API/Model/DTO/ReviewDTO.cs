namespace Vortex_API.Model.DTO
{
    public class ReviewDTO
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? FullName { get; set; }
    }
}

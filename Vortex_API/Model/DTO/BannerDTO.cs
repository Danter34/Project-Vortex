namespace Vortex_API.Model.DTO
{
    public class BannerDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public IFormFile ImageFiles { get; set; }
    }
    public class EdditBannerDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
    }

}

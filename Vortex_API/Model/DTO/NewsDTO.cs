using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.DTO
{
    public class NewsDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, Url]
        public string Link { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
    public class EdditNewsDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, Url]
        public string Link { get; set; }
    }


}

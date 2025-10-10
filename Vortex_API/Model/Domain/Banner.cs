using System.ComponentModel.DataAnnotations;

namespace Vortex_API.Model.Domain
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        // Đường dẫn ảnh đã upload
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

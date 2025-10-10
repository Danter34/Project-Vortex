using Microsoft.EntityFrameworkCore;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Repositories.Service
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ImageRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Image>> GetByProductIdAsync(int productId)
        {
            return await _context.Images
                .Where(i => i.ProductId == productId)
                .ToListAsync();
        }

        public async Task<Image?> UploadImage(int productId, ImageUploadDTO dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadPath = Path.Combine(webRoot, "uploads", "products");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await dto.File.CopyToAsync(stream);

            var img = new Image
            {
                ProductId = product.Id,
                FileName = fileName,
                FileExtension = Path.GetExtension(dto.File.FileName),
                FileSizeInBytes = dto.File.Length,
                FilePath = $"/uploads/products/{fileName}"
            };

            _context.Images.Add(img);
            await _context.SaveChangesAsync();

            return img;
        }

        public async Task<bool> DeleteImage(int imageId)
        {
            var img = await _context.Images.FindAsync(imageId);
            if (img == null) return false;

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webRoot, img.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            _context.Images.Remove(img);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

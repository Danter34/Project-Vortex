using Microsoft.EntityFrameworkCore;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Repositories.Service
{
    public class BannerRepository : IBannerRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BannerRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<Banner>> GetAllBanner()
        {
            return await _context.Banners.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<Banner?> GetBannerById(int id)
        {
            return await _context.Banners.FindAsync(id);
        }

        public async Task<Banner> CreateBanner(BannerDTO dto)
        {
            var banner = new Banner
            {
                Title = dto.Title,
                Description = dto.Description,
            };

            if (dto.ImageFiles != null)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "banners");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFiles.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.ImageFiles.CopyToAsync(stream);

                banner.ImageUrl = $"/uploads/banners/{fileName}";
            }

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();
            return banner;
        }

        public async Task<Banner?> UpdateBanner(int id, EdditBannerDTO dto)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return null;

            banner.Title = dto.Title;
            banner.Description = dto.Description;

            await _context.SaveChangesAsync();
            return banner;
        }

        public async Task<bool> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FirstOrDefaultAsync(n => n.Id == id);
            if (banner == null) return false;

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                var fullPath = Path.Combine(webRoot, banner.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

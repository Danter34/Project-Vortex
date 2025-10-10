using Microsoft.EntityFrameworkCore;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Repositories.Service
{
    public class NewsRepository:INewsRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public NewsRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IEnumerable<News>> GetAllNews()
        {
            return await _context.News.OrderByDescending(n => n.CreatedAt).ToListAsync();
        }

        public async Task<News?> GetNewsById(int id)
        {
            return await _context.News.FindAsync(id);
        }

        public async Task<News> CreateNews(NewsDTO dto)
        {
            var news = new News
            {
                Title = dto.Title,
                Description = dto.Description,
                Link = dto.Link
            };

            if (dto.ImageFile != null)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "news");
                Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await dto.ImageFile.CopyToAsync(stream);

                news.ImageUrl = $"/uploads/news/{fileName}";
            }

            _context.News.Add(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<News?> UpdateNewsbyId(int id, EdditNewsDTO dto)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return null;

            news.Title = dto.Title;
            news.Description = dto.Description;
            news.Link = dto.Link;

            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<bool> DeleteNews(int id)
        {
            var news = await _context.News.FirstOrDefaultAsync(n => n.Id == id);
            if (news == null) return false;

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            if (!string.IsNullOrEmpty(news.ImageUrl))
            {
                var fullPath = Path.Combine(webRoot, news.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

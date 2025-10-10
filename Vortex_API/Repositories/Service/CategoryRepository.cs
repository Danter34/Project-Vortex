using Microsoft.EntityFrameworkCore;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Repositories.Service
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategory(
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        bool isAscending = true,
        int pageNumber = 1,
        int pageSize = 10)
        {
            var query = _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .AsQueryable();

            //  Lọc
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                switch (filterOn.ToLower())
                {
                    case "name":
                        query = query.Where(c => c.Name.Contains(filterQuery));
                        break;
                }
            }

            //  Sắp xếp
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = isAscending ? query.OrderBy(c => c.Name) : query.OrderByDescending(c => c.Name);
                        break;
                    case "id":
                        query = isAscending ? query.OrderBy(c => c.Id) : query.OrderByDescending(c => c.Id);
                        break;
                    default:
                        query = query.OrderBy(c => c.Id);
                        break;
                }
            }

            //  Phân trang
            var skip = (pageNumber - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task<Category?> GetCategoryById(int id)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategory(CategoryDTO categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                ParentId = categoryDto.ParentId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> UpdateCategory(int id, CategoryDTO categoryDto)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return null;

            existing.Name = categoryDto.Name;
            existing.ParentId = categoryDto.ParentId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return false;

            // Xóa danh mục con (nếu có)
            if (category.SubCategories != null && category.SubCategories.Any())
                _context.Categories.RemoveRange(category.SubCategories);

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

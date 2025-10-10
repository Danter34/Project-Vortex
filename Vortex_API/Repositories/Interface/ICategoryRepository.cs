using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategory(string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        bool isAscending = true,
        int pageNumber = 1,
        int pageSize = 1000);
        Task<Category?> GetCategoryById(int id);
        Task<Category> CreateCategory(CategoryDTO categoryDto);
        Task<Category?> UpdateCategory(int id, CategoryDTO categoryDto);
        Task<bool> DeleteCategory(int id);
    }
}

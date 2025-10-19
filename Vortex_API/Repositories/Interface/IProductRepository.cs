using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProduct(string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 100);
        Task<Product?> GetProductById(int id);
        Task<Product> CreateProduct(ProductDTO dto);
        Task<Product?> UpdateProduct(int id, EdditProductDTO dto);
        Task<bool> DeleteProduct(int id);
    }
}

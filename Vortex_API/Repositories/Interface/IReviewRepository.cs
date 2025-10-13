using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetReviewsByProduct(int productId);
        Task<Review?> AddReview(string userId, ReviewDTO dto);
    }
}

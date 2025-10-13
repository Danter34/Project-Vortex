using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface ICartRepository
    {
        Task<Cart> GetCart(string userId);
        Task<Cart> AddItem(string userId, CartItemDTO dto);
        Task<bool> RemoveItem(string userId, int productId);
        Task ClearCart(string userId);
    }
}

using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Vortex_API.Repositories.Service
{
    public class CartRepository:ICartRepository
    {
        private readonly AppDbContext _context;
        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCart(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<Cart> AddItem(string userId, CartItemDTO dto)
        {
            var cart = await GetCart(userId);

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item != null)
            {
                item.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<bool> RemoveItem(string userId, int productId)
        {
            var cart = await GetCart(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ClearCart(string userId)
        {
            var cart = await GetCart(userId);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }
    }
}

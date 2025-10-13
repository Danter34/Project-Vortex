using Microsoft.EntityFrameworkCore;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;
namespace Vortex_API.Repositories.Service
{
    public class OrderRepository:IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly ICartRepository _cartRepository;

        public OrderRepository(AppDbContext context, ICartRepository cartRepository)
        {
            _context = context;
            _cartRepository = cartRepository;
        }

        public async Task<Order> CreateOrder(string userId, OrderDTO dto)
        {
            var cart = await _cartRepository.GetCart(userId);

            if (!cart.Items.Any())
                throw new Exception("Cart is empty.");

            var total = cart.Items.Sum(i => (i.Product?.Price ?? 0M) * i.Quantity);
            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = total,
                Status = "Pending",
                ShippingAddress = dto.ShippingAddress,
                Name = dto.Name,
                Phone = dto.Phone,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product?.Price ?? 0M
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _cartRepository.ClearCart(userId);

            return order;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUser(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderDetail(int orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }
        public async Task<Order?> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return null;

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return order;
        }
        public async Task<IEnumerable<Order>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}

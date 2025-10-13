using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(string userId, OrderDTO dto);
        Task<IEnumerable<Order>> GetOrdersByUser(string userId);
        Task<Order?> GetOrderDetail(int orderId, string userId);
        Task<Order?> UpdateOrderStatus(int orderId, string newStatus);
        Task<IEnumerable<Order>> GetAllOrders();
    }
}

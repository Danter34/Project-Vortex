using Vortex_API.Model.Domain;
using Vortex_API.Model.DTO;

namespace Vortex_API.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(string userId, OrderDTO dto);
        Task<IEnumerable<OrderDTOView>> GetOrdersByUser(string userId, int page = 1, int pageSize = 5);
        Task<OrderDTOView?> GetOrderDetail(int orderId);
        Task<Order?> UpdateOrderStatus(int orderId, string newStatus);
        Task<IEnumerable<OrderDTOView>> GetAllOrders(int page = 1, int pageSize = 100);
        Task<Order?> CancelOrder(int orderId, string userId);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Vortex_API.Model.DTO;
using Vortex_API.Repositories.Interface;

namespace Vortex_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderRepository orderRepository, ILogger<OrderController> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-order")]
        public async Task<IActionResult> GetAllOrders(int page = 1, int pageSize = 10)
        {
            _logger.LogInformation("Admin is fetching all orders, page {Page}, pageSize {PageSize}.", page, pageSize);
            var orders = await _orderRepository.GetAllOrders(page, pageSize);
            _logger.LogInformation("{Count} orders retrieved.", orders.Count());
            return Ok(orders);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _logger.LogInformation("User {UserId} is creating an order.", userId);

                var order = await _orderRepository.CreateOrder(userId, dto);

                _logger.LogInformation("Order {OrderId} created successfully for user {UserId}.", order.Id, userId);
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order.");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders(int page = 1, int pageSize = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Fetching orders for user {UserId}, page {Page}, pageSize {PageSize}.", userId, page, pageSize);

            var orders = await _orderRepository.GetOrdersByUser(userId, page, pageSize);

            _logger.LogInformation("{Count} orders retrieved for user {UserId}.", orders.Count(), userId);
            return Ok(orders);
        }

        [Authorize]
        [HttpGet("get-order-detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Fetching details for order {OrderId} by user {UserId}.", id, userId);

            var order = await _orderRepository.GetOrderDetail(id, userId);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for user {UserId}.", id, userId);
                return NotFound(new { message = "Order not found" });
            }

            _logger.LogInformation("Order {OrderId} retrieved successfully for user {UserId}.", id, userId);
            return Ok(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            _logger.LogInformation("Admin updating status for order {OrderId} to {NewStatus}.", id, newStatus);

            var updatedOrder = await _orderRepository.UpdateOrderStatus(id, newStatus);
            if (updatedOrder == null)
            {
                _logger.LogWarning("Order {OrderId} not found for status update.", id);
                return NotFound(new { message = "Order not found" });
            }

            _logger.LogInformation("Order {OrderId} status updated to {NewStatus} successfully.", id, newStatus);
            return Ok(new
            {
                message = "Order status updated successfully",
                order = updatedOrder
            });
        }
        [Authorize]
        [HttpPost("cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var cancelledOrder = await _orderRepository.CancelOrder(orderId, userId);
                if (cancelledOrder == null)
                    return NotFound(new { message = "Đơn hàng không tồn tại." });

                return Ok(new { message = "Đơn hàng đã hủy thành công.", order = cancelledOrder });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, userId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

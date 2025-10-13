using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-order")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllOrders();
            return Ok(orders);
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _orderRepository.CreateOrder(userId, dto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderRepository.GetOrdersByUser(userId);
            return Ok(orders);
        }

        [Authorize]
        [HttpGet("get-order-detail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderRepository.GetOrderDetail(id, userId);
            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string newStatus)
        {
            var updatedOrder = await _orderRepository.UpdateOrderStatus(id, newStatus);
            if (updatedOrder == null)
                return NotFound(new { message = "Order not found" });

            return Ok(new
            {
                message = "Order status updated successfully",
                order = updatedOrder
            });
        }
    }
}

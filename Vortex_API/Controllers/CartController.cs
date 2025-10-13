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
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("get-cart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartRepository.GetCart(userId);
            return Ok(cart);
        }

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddItem([FromBody] CartItemDTO dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _cartRepository.AddItem(userId, dto);
            return Ok(cart);
        }

        [HttpDelete("remove-cart/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var success = await _cartRepository.RemoveItem(userId, productId);
            if (!success) return NotFound();

            return Ok(new { Message = "Item removed successfully" });
        }

        [HttpDelete("clear-cart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            await _cartRepository.ClearCart(userId);
            return Ok(new { Message = "Cart cleared successfully" });
        }
    }
}

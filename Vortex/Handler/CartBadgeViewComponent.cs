using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vortex.Models;

namespace Vortex.Handler
{
    public class CartBadgeViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartBadgeViewComponent(IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = accessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient("APIClient");

            // Lấy JWT từ session
            var token = _httpContextAccessor.HttpContext.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var cartResponse = await client.GetAsync("https://localhost:7161/api/Cart/get-cart");
            int cartCount = 0;
            if (cartResponse.IsSuccessStatusCode)
            {
                var cartContent = await cartResponse.Content.ReadAsStringAsync();
                var cart = JsonSerializer.Deserialize<CartViewModel>(cartContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                cartCount = cart?.Items?.Sum(i => i.Quantity) ?? 0;
            }

            return View(cartCount); // trả về số lượng
        }
    }
}

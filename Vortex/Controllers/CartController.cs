using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Vortex.Models;
using Microsoft.AspNetCore.Authorization;

namespace Vortex.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _cartApiBase = "https://localhost:7161/api/Cart/";

        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        private int GetCartCountFromCookie()
        {
            if (Request.Cookies.TryGetValue("CartCount", out var value) && int.TryParse(value, out int count))
                return count;
            return 0;
        }

        private void SetCartCountCookie(int count)
        {
            Response.Cookies.Append("CartCount", count.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddYears(5), // cookie tồn tại lâu dài
                HttpOnly = true,
                IsEssential = true
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var dto = new CartItemDTO { ProductId = id, Quantity = quantity };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_cartApiBase + "add-to-cart", content);

            if (response.IsSuccessStatusCode)
            {
                int cartCount = GetCartCountFromCookie();
                cartCount += quantity;
                SetCartCountCookie(cartCount);

                TempData["CartSuccess"] = "Thêm giỏ hàng thành công!";
            }
            else
            {
                TempData["CartError"] = "Thêm giỏ hàng thất bại!";
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartResponse = await _httpClient.GetAsync(_cartApiBase + "get-cart");
            if (!cartResponse.IsSuccessStatusCode)
                return View(new CartViewModel());

            var cartContent = await cartResponse.Content.ReadAsStringAsync();
            var cart = JsonSerializer.Deserialize<CartViewModel>(cartContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CartViewModel();

            var cartViewModel = new CartViewModel();
            foreach (var item in cart.Items)
            {
                var productResponse = await _httpClient.GetAsync($"https://localhost:7161/api/Product/get-product-by-id/{item.ProductId}");
                if (!productResponse.IsSuccessStatusCode) continue;

                var productContent = await productResponse.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductViewModel>(productContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (product != null)
                {
                    cartViewModel.Items.Add(new CartItemViewModel
                    {
                        ProductId = product.Id,
                        ProductTitle = product.Title,
                        ProductImage = product.Images.FirstOrDefault()?.FilePath,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }
            }

            // Đảm bảo cookie đồng bộ với số lượng trong giỏ API
            SetCartCountCookie(cart.Items.Sum(i => i.Quantity));

            return View(cartViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var response = await _httpClient.DeleteAsync(_cartApiBase + $"remove-cart/{productId}");
            if (response.IsSuccessStatusCode)
            {
                int cartCount = GetCartCountFromCookie();
                cartCount = Math.Max(cartCount - 1, 0);
                SetCartCountCookie(cartCount);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var response = await _httpClient.DeleteAsync(_cartApiBase + "clear-cart");
            if (response.IsSuccessStatusCode)
            {
                SetCartCountCookie(0);
            }
            return RedirectToAction("Index");
        }
    }
}

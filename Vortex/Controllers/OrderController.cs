using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/Order/";

        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}get-all-order?page={page}&pageSize=10");
            if (!response.IsSuccessStatusCode) return View(new List<MyOrderViewModel>());

            var json = await response.Content.ReadAsStringAsync();

            // Deserialize với case-insensitive để map id → OrderId
            var orders = JsonSerializer.Deserialize<List<MyOrderViewModel>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<MyOrderViewModel>();

            return View(orders);
        }
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var response = await _httpClient.GetAsync($"{_baseUrl}get-order-detail/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();

            // Deserialize JSON từ API sang DTO MVC
            var orderDto = JsonSerializer.Deserialize<OrderDTOView_MVC>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (orderDto == null) return NotFound();

            // Map DTO API sang ViewModel MVC
            var order = new MyOrderViewModel
            {
                OrderId = orderDto.Id,
                CreatedAt = orderDto.CreatedAt,
                Status = orderDto.Status,
                Name = orderDto.UserInfo.FirstOrDefault()?.Name ?? "",
                ShippingAddress = orderDto.UserInfo.FirstOrDefault()?.ShippingAddress ?? "",
                Phone = orderDto.UserInfo.FirstOrDefault()?.Phone ?? 0,
                Items = orderDto.Items.Select(i => new MyOrderItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductTitle = i.ProductTitle,
                    ProductImage = i.ProductImage,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            var content = new StringContent(JsonSerializer.Serialize(newStatus), Encoding.UTF8, "application/json");
             await _httpClient.PutAsync($"{_baseUrl}update-status/{orderId}", content);

            return RedirectToAction("Index", new { id = orderId });
        }
    }
}

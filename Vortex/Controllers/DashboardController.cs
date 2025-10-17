using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public DashboardController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            int totalProducts = 0;
            int totalOrders = 0;
            int totalUsers = 0;
            int adminUsersCount = 0;
            int pendingFeedbackCount = 0;

            var client = _clientFactory.CreateClient("APIClient");
            client.BaseAddress = new Uri("https://localhost:7161/"); // API base URL
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

 
            try
            {
                // 1. Tổng sản phẩm
                var productsResp = await client.GetAsync("api/Product/get-all-product");
                if (productsResp.IsSuccessStatusCode)
                {
                    // Nếu API gửi header X-Total-Count
                    if (productsResp.Headers.TryGetValues("X-Total-Count", out var prodValues))
                        totalProducts = int.Parse(prodValues.FirstOrDefault() ?? "0");
                    else
                    {
                        var productsJson = await productsResp.Content.ReadAsStringAsync();
                        var productsList = JsonConvert.DeserializeObject<List<object>>(productsJson);
                        totalProducts = productsList?.Count ?? 0;
                    }
                }

                // 2. Tổng đơn hàng
                var ordersResp = await client.GetAsync("api/Order/get-all-order");
                if (ordersResp.IsSuccessStatusCode)
                {
                    if (ordersResp.Headers.TryGetValues("X-Total-Count", out var orderValues))
                        totalOrders = int.Parse(orderValues.FirstOrDefault() ?? "0");
                    else
                    {
                        var ordersJson = await ordersResp.Content.ReadAsStringAsync();
                        var ordersList = JsonConvert.DeserializeObject<List<object>>(ordersJson);
                        totalOrders = ordersList?.Count ?? 0;
                    }
                }

                // 3. Tổng user + số admin
                var usersResp = await client.GetAsync("api/UserManagement");
                if (usersResp.IsSuccessStatusCode)
                {
                    var usersJson = await usersResp.Content.ReadAsStringAsync();
                    var usersList = JsonConvert.DeserializeObject<List<UserViewModel>>(usersJson);
                    totalUsers = usersList?.Count(u => u.Roles.Contains("User")) ?? 0;
                    adminUsersCount = usersList?.Count(u => u.Roles.Contains("Admin")) ?? 0;
                }

                // 4. Số phản hồi pending
                var feedbackResp = await client.GetAsync("api/Feedback/all");
                if (feedbackResp.IsSuccessStatusCode)
                {
                    var feedbackJson = await feedbackResp.Content.ReadAsStringAsync();
                    var feedbackList = JsonConvert.DeserializeObject<List<FeedbackResponseViewModel>>(feedbackJson);
                    pendingFeedbackCount = feedbackList?.Count(f => f.Status != "Replied") ?? 0;
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu muốn
                Console.WriteLine(ex.Message);
            }

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalDeliveries = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.AdminUsersCount = adminUsersCount;
            ViewBag.PendingFeedbackCount = pendingFeedbackCount;

            return View();
        }
    }
}

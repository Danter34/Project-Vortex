using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/UserManagement/";

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
        {
            // Tạo URL truy vấn API
            string url = $"{_baseUrl}?filterOn=fullname&filterQuery={search}&pageNumber={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Không thể tải danh sách người dùng.";
                return View(new List<UserViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var users = JsonSerializer.Deserialize<List<UserViewModel>>(json, options) ?? new List<UserViewModel>();

            // Nếu không có dữ liệu thì báo
            if (users == null || users.Count == 0)
            {
                return View(new List<UserViewModel>());
            }

            // Phân trang cơ bản (nếu muốn hiển thị nút next/prev)
            ViewBag.CurrentPage = page;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = users.Count == pageSize;
            ViewBag.Search = search;

            return View(users);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateUserRoleViewModel model)
        {
            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}update-role", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Cập nhật role thất bại.";
            }
            else
            {
                TempData["Success"] = "Cập nhật role thành công!";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}delete/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Xóa user thất bại.";
            }
            else
            {
                TempData["Success"] = "Xóa user thành công!";
            }

            return RedirectToAction("Index");
        }
    }
}

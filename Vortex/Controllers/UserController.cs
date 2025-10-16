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

        // ====== GET ALL USERS ======
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}all");
            if (!response.IsSuccessStatusCode) return View(new List<UserViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<UserViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<UserViewModel>();

            return View(users);
        }

        // ====== UPDATE ROLE ======
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

        // ====== DELETE USER ======
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

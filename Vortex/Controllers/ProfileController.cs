using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            client.BaseAddress = new Uri("https://localhost:7161/api/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync("Auth/profile");
            if (!response.IsSuccessStatusCode)
                return View(new ProfileViewModel());

            var json = await response.Content.ReadAsStringAsync();
            var profile = System.Text.Json.JsonSerializer.Deserialize<ProfileViewModel>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new ProfileViewModel();

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel dto)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            client.BaseAddress = new Uri("https://localhost:7161/api/");

            var response = await client.PutAsJsonAsync("Auth/change-password", dto);
            if (response.IsSuccessStatusCode)
                return Ok(new { message = "Cập nhật mật khẩu thành công!" });

            var error = await response.Content.ReadAsStringAsync();
            return BadRequest(error);
        }
        public IActionResult MyOrdersRedirect()
        {
            return RedirectToAction("MyOrders", "Checkout");
        }
    }
}

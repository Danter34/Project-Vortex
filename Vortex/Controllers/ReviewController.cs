using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Vortex.Model;
namespace Vortex.Controllers
{
    public class ReviewController : Controller
    {
        private readonly HttpClient _httpClient;

        public ReviewController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient"); 
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(ReviewDTO dto)
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7161/api/Review/add-review", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["ReviewSuccess"] = "Đánh giá đã được gửi thành công!";
            }
            else
            {
                var json = await response.Content.ReadAsStringAsync();
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    TempData["ReviewError"] = errorObj?["message"] ?? "Có lỗi xảy ra khi gửi đánh giá.";
                }
                catch
                {
                    TempData["ReviewError"] = "Có lỗi xảy ra khi gửi đánh giá.";
                }
            }

            return RedirectToAction("Detail", "Home", new { id = dto.ProductId });
        }
    }

}
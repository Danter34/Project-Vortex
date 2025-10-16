using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/Feedback/";

        public FeedbackController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        // User gửi phản hồi + xem phản hồi gần nhất
        public async Task<IActionResult> Index()
        {
            // Lấy phản hồi gần nhất của user
            var response = await _httpClient.GetAsync($"{_baseUrl}my-feedback");
            FeedbackResponseViewModel? feedback = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                feedback = JsonSerializer.Deserialize<FeedbackResponseViewModel>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return View(feedback);
        }

        [HttpPost]
        public async Task<IActionResult> SendFeedback(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Bạn phải nhập nội dung phản hồi.";
                return RedirectToAction("Index");
            }

            var dto = new FeedbackDTO { Message = message };
            var content = new StringContent(JsonSerializer.Serialize(dto), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl, content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Phản hồi đã gửi thành công!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = error;
            }

            return RedirectToAction("Index");
        }

        // Admin: xem tất cả feedbacks
        public async Task<IActionResult> AdminIndex()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}all");
            List<FeedbackResponseViewModel> feedbacks = new();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                feedbacks = JsonSerializer.Deserialize<List<FeedbackResponseViewModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FeedbackResponseViewModel>();
            }

            return View(feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> Reply(int id, string reply)
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                TempData["Error"] = "Bạn phải nhập nội dung trả lời.";
                return RedirectToAction("AdminIndex");
            }

            var content = new StringContent(JsonSerializer.Serialize(reply), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}reply/{id}", content);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Đã trả lời phản hồi thành công.";
            else
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction("AdminIndex");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}delete/{id}");
            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Đã xóa phản hồi.";
            else
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction("AdminIndex");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class NewsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/News/";

        public NewsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        // GET: /News
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}get-all-news");
            if (!response.IsSuccessStatusCode) return View(new List<NewsViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var newsList = JsonSerializer.Deserialize<List<NewsViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NewsViewModel>();

            return View(newsList);
        }

        // GET: /News/Create
        public IActionResult Create() => View();

        // POST: /News/Create
        [HttpPost]
        public async Task<IActionResult> Create(NewsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Title), "Title");
            content.Add(new StringContent(model.Description ?? ""), "Description");
            content.Add(new StringContent(model.Link), "Link");

            if (model.ImageFile != null)
            {
                var streamContent = new StreamContent(model.ImageFile.OpenReadStream());
                content.Add(streamContent, "ImageFile", model.ImageFile.FileName);
            }

            var response = await _httpClient.PostAsync($"{_baseUrl}add-news", content);
            if (response.IsSuccessStatusCode) return RedirectToAction("Index");

            ModelState.AddModelError("", "Thêm tin tức thất bại");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"{_baseUrl}delete-news/{id}");
            return RedirectToAction("Index");
        }
    }
}

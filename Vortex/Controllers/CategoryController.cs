using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class CategoryController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/Category/";

        public CategoryController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }


        public async Task<IActionResult> Index(int page = 1)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}get-all-category?pageNumber={page}&pageSize=20");
            if (!response.IsSuccessStatusCode) return View(new List<CategoryViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var categories = JsonSerializer.Deserialize<List<CategoryViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CategoryViewModel>();

            return View(categories);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}add-category", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Tạo danh mục thất bại");
            return View(model);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}get-category-by-id/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var category = JsonSerializer.Deserialize<CategoryViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (category == null) return NotFound();
            return View(category);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}update-category-by-id/{model.Id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Cập nhật danh mục thất bại");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}delete-category/{id}");
            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class BannerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/Banner/";

        public BannerController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        // ====== GET ALL BANNERS ======
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}Get-all-banner");
            if (!response.IsSuccessStatusCode) return View(new List<BannerViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var banners = JsonSerializer.Deserialize<List<BannerViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<BannerViewModel>();

            return View(banners);
        }

        // ====== CREATE BANNER ======
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(BannerViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Title), "Title");
            if (!string.IsNullOrEmpty(model.Description))
                content.Add(new StringContent(model.Description), "Description");
            if (model.ImageFile != null)
                content.Add(new StreamContent(model.ImageFile.OpenReadStream()), "ImageFiles", model.ImageFile.FileName);

            var response = await _httpClient.PostAsync($"{_baseUrl}add-banner", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Thêm banner thất bại";
                return View(model);
            }

            TempData["Success"] = "Thêm banner thành công";
            return RedirectToAction("Index");
        }

       

        // ====== DELETE BANNER ======
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}delete-banner-by-id/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Xóa banner thất bại";
            }
            else
            {
                TempData["Success"] = "Xóa banner thành công";
            }

            return RedirectToAction("Index");
        }
    }
}

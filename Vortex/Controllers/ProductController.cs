using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/Product/";
        private readonly string _categoryUrl = "https://localhost:7161/api/Category/";
        private readonly string _imageUrl = "https://localhost:7161/api/Image/";

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}get-all-product?pageNumber={pageNumber}&pageSize=10");
            if (!response.IsSuccessStatusCode) return View(new List<ProductViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<ProductViewModel>>(json);

            ViewBag.PageNumber = pageNumber;
            return View(data);
        }

        // ========== CREATE ==========
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categoryResponse = await _httpClient.GetAsync($"{_categoryUrl}get-all-category");
            var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(categoryJson);

            ViewBag.Categories = categories ?? new List<CategoryViewModel>();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateModel model)
        {
            var formData = new MultipartFormDataContent
            {
                { new StringContent(model.Title), "Title" },
                { new StringContent(model.Price.ToString()), "Price" },
                { new StringContent(model.StockQuantity.ToString()), "StockQuantity" },
                { new StringContent(model.CategoryId.ToString()), "CategoryId" },
                { new StringContent(model.Description ?? ""), "Description" },
                { new StringContent(model.IsHot.ToString()), "IsHot" },
                { new StringContent(model.IsNew.ToString()), "IsNew" }
            };

            if (model.Images != null)
            {
                foreach (var img in model.Images)
                {
                    formData.Add(new StreamContent(img.OpenReadStream()), "Images", img.FileName);
                }
            }

            var response = await _httpClient.PostAsync($"{_baseUrl}add-product", formData);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "❌ Lỗi khi thêm sản phẩm");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}get-product-by-id/{id}");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<ProductViewModel>(json);

            // Lấy danh sách category
            var categoryResponse = await _httpClient.GetAsync($"{_categoryUrl}get-all-category");
            var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
            var categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(categoryJson);

            ViewBag.Categories = categories ?? new List<CategoryViewModel>();

            var model = new ProductEditModel
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                Description = product.Description,
                IsHot = product.IsHot,
                IsNew = product.IsNew
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductEditModel model)
        {
            if (!ModelState.IsValid)
            {
                // reload categories nếu form bị lỗi
                var categoryResponse = await _httpClient.GetAsync($"{_categoryUrl}get-all-category");
                var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                var categories = JsonConvert.DeserializeObject<List<CategoryViewModel>>(categoryJson);
                ViewBag.Categories = categories ?? new List<CategoryViewModel>();

                return View(model);
            }

            // Gửi JSON tới API
            var jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_baseUrl}update-product-by-id/{model.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "✅ Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "❌ Lỗi khi cập nhật sản phẩm");

            // reload categories nếu lỗi
            var catResp = await _httpClient.GetAsync($"{_categoryUrl}get-all-category");
            var catJson = await catResp.Content.ReadAsStringAsync();
            var cats = JsonConvert.DeserializeObject<List<CategoryViewModel>>(catJson);
            ViewBag.Categories = cats ?? new List<CategoryViewModel>();

            return View(model);
        }



        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"{_baseUrl}delete-product/{id}");
            return RedirectToAction("Index");
        }
        
    }
}

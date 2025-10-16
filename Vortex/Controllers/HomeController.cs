using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7161/api/");
        }

        public async Task<IActionResult> Index()
        {
            var banners = await GetAsync<List<BannerViewModel>>("Banner/Get-all-banner");
            var hotProducts = await GetAsync<List<ProductViewModel>>("Product/get-all-product?filterOn=IsHot&filterQuery=true");
            var newProducts = await GetAsync<List<ProductViewModel>>("Product?filterOn=IsNew&filterQuery=true");
            var news = await GetAsync<List<NewsViewModel>>("News/get-all-news");

            ViewBag.Banners = banners ?? new List<BannerViewModel>();
            ViewBag.HotProducts = hotProducts ?? new List<ProductViewModel>();
            ViewBag.NewProducts = newProducts ?? new List<ProductViewModel>();
            ViewBag.News = news ?? new List<NewsViewModel>();

            return View();
        }

        private async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        public async Task<IActionResult> Detail(int id)
        {
            // Lấy sản phẩm chi tiết
            var product = await GetAsync<ProductViewModel>($"Product/get-product-by-id/{id}");
            if (product == null) return NotFound();

            // Lấy danh sách review
            var reviews = await GetAsync<List<ReviewViewModel>>($"Review/get-review/{id}");
            product.Reviews = reviews ?? new List<ReviewViewModel>();

            return View(product);
        }
        public async Task<IActionResult> Filter(
    int? categoryId,
    string? search,
    string? sortOption,
    int pageNumber = 1,
    int pageSize = 8)
        {
            bool isAscending = true;
            string? sortBy = null;

            switch (sortOption)
            {
                case "priceAsc":
                    sortBy = "price";
                    isAscending = true;
                    break;
                case "priceDesc":
                    sortBy = "price";
                    isAscending = false;
                    break;
                default:
                    sortBy = null; // mặc định
                    break;
            }

            string endpoint = $"Product/get-all-product?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(search))
                endpoint += $"&filterOn=Title&filterQuery={search}";

            if (categoryId.HasValue)
                endpoint += $"&filterOn=categoryid&filterQuery={categoryId.Value}";

            if (!string.IsNullOrEmpty(sortBy))
                endpoint += $"&sortBy={sortBy}&isAscending={isAscending}";

            var products = await GetAsync<List<ProductViewModel>>(endpoint);

            string? categoryName = null;
            if (categoryId.HasValue)
            {
                var category = await GetAsync<CategoryViewModel>($"Category/get-category-by-id/{categoryId.Value}");
                categoryName = category?.Name;
            }

            ViewBag.CurrentCategory = categoryName;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSortOption = sortOption;
            ViewBag.PageNumber = pageNumber;

            return View(products ?? new List<ProductViewModel>());
        }

    }
}

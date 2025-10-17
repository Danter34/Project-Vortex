using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CheckoutController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("APIClient");

            // Lấy giỏ hàng từ API giống phần Cart
            var cartResponse = await client.GetAsync("https://localhost:7161/api/Cart/get-cart");
            if (!cartResponse.IsSuccessStatusCode)
                return View(new CheckoutViewModel());

            var cartContent = await cartResponse.Content.ReadAsStringAsync();
            var cart = System.Text.Json.JsonSerializer.Deserialize<CartViewModel>(cartContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CartViewModel();

            var checkoutModel = new CheckoutViewModel();
            foreach (var item in cart.Items)
            {
                var productResponse = await client.GetAsync($"https://localhost:7161/api/Product/get-product-by-id/{item.ProductId}");
                if (!productResponse.IsSuccessStatusCode) continue;

                var productContent = await productResponse.Content.ReadAsStringAsync();
                var product = System.Text.Json.JsonSerializer.Deserialize<ProductViewModel>(productContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (product != null)
                {
                    checkoutModel.Items.Add(new CartItemViewModel
                    {
                        ProductId = product.Id,
                        ProductTitle = product.Title,
                        ProductImage = product.Images.FirstOrDefault()?.FilePath ?? "/images/default.png",
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }
            }

            // Có thể để trống thông tin nhận hàng hoặc fill mặc định từ user
            checkoutModel.Name = "";
            checkoutModel.Phone = "";
            checkoutModel.Address = "";

            return View(checkoutModel);
        }


        [HttpPost]
        public async Task<IActionResult> Pay(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin.";
                return View("Index", model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Prepare order DTO
            var orderDto = new
            {
                Items = model.Items.Select(i => new { ProductId = i.ProductId, Quantity = i.Quantity }).ToList(),
                Name = model.Name,
                Phone = int.Parse(model.Phone),
                ShippingAddress = model.Address,
                PaymentMethod = model.PaymentMethod
            };

            var client = _httpClientFactory.CreateClient("APIClient");
            client.BaseAddress = new Uri("https://localhost:7161/api/"); // API base
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Call API create order
            var content = new StringContent(JsonConvert.SerializeObject(orderDto), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("Order/create", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Tạo đơn hàng thất bại.";
                return View("Index", model);
            }

            var orderJson = await response.Content.ReadAsStringAsync();
            dynamic order = JsonConvert.DeserializeObject(orderJson)!;
            int orderId = order.id;

            // Nếu COD → chuyển thẳng success
            if (model.PaymentMethod == "COD")
            {
                return RedirectToAction("Success", new { orderId });
            }
            else
            {
                // Nếu MoMo → call API create payment
                var paymentResponseDto = new { OrderId = orderId };
                var paymentContent = new StringContent(JsonConvert.SerializeObject(paymentResponseDto), Encoding.UTF8, "application/json");
                var paymentResponse = await client.PostAsync("Payment/create-momo", paymentContent);

                if (!paymentResponse.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Tạo thanh toán thất bại.";
                    return View("Index", model);
                }

                var paymentJson = await paymentResponse.Content.ReadAsStringAsync();
                dynamic payment = JsonConvert.DeserializeObject(paymentJson)!;

                string payUrl = payment.payUrl;

                return Redirect(payUrl); // Redirect sang MoMo
            }
        }

        // GET: Thanh toán thành công
        public IActionResult Success(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        // GET: Thanh toán thất bại
        public IActionResult Failed(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> MyOrders(int pageNumber = 1, int pageSize = 5)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            client.BaseAddress = new Uri("https://localhost:7161/api/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Gọi API với phân trang
            var response = await client.GetAsync($"Order/my-orders?page={pageNumber}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode)
                return View(new List<MyOrderViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var orders = System.Text.Json.JsonSerializer.Deserialize<List<MyOrderViewModel>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<MyOrderViewModel>();

            // Lọc trạng thái Pending, Shipping, Shipped
            orders = orders.Where(o => o.Status == "Pending" || o.Status == "shipping" || o.Status == "shipped" || o.Status == "Cancelled").ToList();

            // Truyền thông tin phân trang cho ViewBag
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            client.BaseAddress = new Uri("https://localhost:7161/api/");

            var response = await client.PostAsync($"Order/cancel/{orderId}", null); // {id} match API
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Hủy đơn hàng thất bại!";
            }
            else
            {
                TempData["Success"] = "Đơn hàng đã hủy thành công!";
            }

            return RedirectToAction("MyOrders");
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Vortex.Models;

namespace Vortex.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
            _httpClient.BaseAddress = new Uri("https://localhost:7161/api/Auth/");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!response.IsSuccessStatusCode || authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            {
                ViewBag.LoginError = authResponse?.Message ?? "Sai email hoặc mật khẩu!";
                return View(model);
            }

            // Lưu JWT vào session
            HttpContext.Session.SetString("JWToken", authResponse.Token);

            // Giải mã token để lấy role
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(authResponse.Token);
            var roles = jwtToken.Claims
                .Where(c => c.Type == "role" || c.Type.EndsWith("/role"))
                .Select(c => c.Value)
                .ToList();

            // Lưu role vào session
            HttpContext.Session.SetString("UserRoles", string.Join(",", roles));

            // Cookie authentication với persistent cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim("JWT", authResponse.Token)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // giữ login khi đóng trình duyệt
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) // cookie tồn tại 30 ngày
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("UserRoles");
            Response.Cookies.Delete("CartCount");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}

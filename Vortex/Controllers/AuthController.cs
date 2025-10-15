using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
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
            if (!ModelState.IsValid) return View(model);

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

            // Cookie authentication cho MVC (session-only)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim("JWT", authResponse.Token)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true 
                });

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JWToken");

            Response.Cookies.Delete("CartCount");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

    }
}

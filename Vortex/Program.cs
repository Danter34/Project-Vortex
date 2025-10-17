using Microsoft.AspNetCore.Authentication.Cookies;
using Vortex.Handler;

var builder = WebApplication.CreateBuilder(args);

// HttpClient + JWT handler
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtTokenHandler>();
builder.Services.AddHttpClient("APIClient")
       .AddHttpMessageHandler<JwtTokenHandler>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Cookie authentication cho MVC
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // trang login
        options.LogoutPath = "/Auth/Logout"; // trang logout
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // giữ login 30 ngày
        options.SlidingExpiration = true; // tự động gia hạn khi hoạt động
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "VortexAuthCookie"; // tên cookie
    });
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

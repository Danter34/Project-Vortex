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
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // đường dẫn login
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // session tự hết sau 15 phút
        options.SlidingExpiration = true; // nếu user hoạt động thì refresh timeout
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
// MVC
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

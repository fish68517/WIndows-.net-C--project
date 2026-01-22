using Microsoft.EntityFrameworkCore;
using TourismPlatform.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// 1. 配置数据库连接
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. 配置 Identity (用户认证)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<MyDbContext>();

// 3. 添加 Session 支持 (为了保存登录状态和简单的购物车)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. 添加 MVC 控制器和视图支持
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 5. 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHttpsRedirection(); // 注释掉强制 HTTPS 跳转，避免生产环境警告
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 认证
app.UseAuthorization();  // 授权

app.UseSession(); // 开启 Session

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
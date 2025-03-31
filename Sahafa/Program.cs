using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sahafa.Data;
using Sahafa.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Thêm dịch vụ cần thiết trước khi Build()
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// 🔹 Kết nối SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);



builder.Services.AddScoped<AuthService>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 🔹 Cấu hình Identity (KHÔNG yêu cầu xác thực Email)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 🔹 Thêm Authorization Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Manager", "Staff"));
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
});

// 🔹 Đăng ký Controllers + Views
builder.Services.AddControllersWithViews();

// 🔹 Tạo ứng dụng sau khi đã đăng ký hết dịch vụ
var app = builder.Build();

// 🔹 Cấu hình Middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  // Đảm bảo phục vụ file tĩnh (CSS, JS, Images)

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

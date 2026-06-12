using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using CivicConnect.Web.Data;
using CivicConnect.Web.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình MVC
builder.Services.AddControllersWithViews();

// Cấu hình giới hạn upload file (tối đa 5 ảnh x 5MB = 25MB + overhead)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
    options.ValueLengthLimit = 10 * 1024 * 1024;         // 10 MB
    options.MultipartHeadersLengthLimit = 32 * 1024;     // 32 KB
});

// Cấu hình Kestrel cho phép request body lớn
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

// Cấu hình Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CivicConnect.Web")));

// Cấu hình ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Cấu hình khóa tài khoản
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình tài khoản
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Tắt yêu cầu confirm email mặc định để tiện thử nghiệm
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Cookie Authentication cho MVC Views
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Đăng ký các Repository và Service chuyên biệt
builder.Services.Configure<CivicConnect.Web.Models.CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IMomoService, MomoService>();

// Đăng ký các Hosted Services (Background Jobs chạy nền)
builder.Services.AddHostedService<PriorityScoreJob>();
builder.Services.AddHostedService<DeadlineCheckJob>();

// Đăng ký SignalR để gửi thông báo realtime
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

// Tự động chạy Seed Data cho các tài khoản thử nghiệm khi khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await DbInitializer.SeedUsersAsync(userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra khi chạy seeding dữ liệu tài khoản mẫu.");
    }
}

// Cấu hình HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Cấu hình Route cho Area Admin
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Admin}/{action=Dashboard}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<DonationHub>("/hubs/donation");

app.Run();

public partial class Program { }

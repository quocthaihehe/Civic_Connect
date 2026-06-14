using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using CivicConnect.Web.Services.BackgroundJobs;
using CivicConnect.Web.Data;
using CivicConnect.Web.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Cáº¥u hÃ¬nh MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Cáº¥u hÃ¬nh giá»›i háº¡n upload file (tá»‘i Ä‘a 5 áº£nh x 5MB = 25MB + overhead)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
    options.ValueLengthLimit = 10 * 1024 * 1024;         // 10 MB
    options.MultipartHeadersLengthLimit = 32 * 1024;     // 32 KB
});

// Cáº¥u hÃ¬nh Kestrel cho phÃ©p request body lá»›n
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

// Cấu hình Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CivicConnect.Web")));

// Cáº¥u hÃ¬nh ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cáº¥u hÃ¬nh máº­t kháº©u
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;

    // Cáº¥u hÃ¬nh khÃ³a tÃ i khoáº£n
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Cáº¥u hÃ¬nh tÃ i khoáº£n
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Táº¯t yÃªu cáº§u confirm email máº·c Ä‘á»‹nh Ä‘á»ƒ tiá»‡n thá»­ nghiá»‡m
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Cáº¥u hÃ¬nh Cookie Authentication cho MVC Views
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
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

// AI Service — Gemini
builder.Services.Configure<CivicConnect.Web.Models.GeminiSettings>(
    builder.Configuration.GetSection("GeminiSettings"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiService, GeminiAiService>();

// Ä Äƒng kÃ½ cÃ¡c Hosted Services (Background Jobs cháº¡y ná» n)
builder.Services.AddHostedService<PriorityScoreJob>();
builder.Services.AddHostedService<DeadlineCheckJob>();

// Ä Äƒng kÃ½ SignalR Ä‘á»ƒ gá»­i thÃ´ng bÃ¡o realtime
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

// Tá»± Ä‘á»™ng cháº¡y Seed Data cho cÃ¡c tÃ i khoáº£n thá»­ nghiá»‡m khi khá»Ÿi Ä‘á»™ng
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await DbInitializer.SeedUsersAsync(userManager);
        
        // Reset online status of all users to offline at startup
        var dbContext = services.GetRequiredService<AppDbContext>();
        var onlineUsers = await dbContext.Users.Where(u => u.IsOnline).ToListAsync();
        if (onlineUsers.Any())
        {
            foreach (var user in onlineUsers)
            {
                user.IsOnline = false;
            }
        }

        // Auto-update policy SourceUrls to real working Tuoi Tre article links to bypass geoblocking on Somee
        bool hasChanges = false;
        var policy1 = await dbContext.Policies.FindAsync(1);
        if (policy1 != null && policy1.SourceUrl != "https://tuoitre.vn/vut-rac-bua-bai-tieu-bay-se-bi-phat-gap-10-lan-1259236.htm")
        {
            policy1.SourceUrl = "https://tuoitre.vn/vut-rac-bua-bai-tieu-bay-se-bi-phat-gap-10-lan-1259236.htm";
            hasChanges = true;
        }
        var policy2 = await dbContext.Policies.FindAsync(2);
        if (policy2 != null && policy2.SourceUrl != "https://tuoitre.vn/de-xuat-xay-cong-vien-khoa-hoc-cho-thieu-nhi-tai-tp-hcm-20240906132157012.htm")
        {
            policy2.SourceUrl = "https://tuoitre.vn/de-xuat-xay-cong-vien-khoa-hoc-cho-thieu-nhi-tai-tp-hcm-20240906132157012.htm";
            hasChanges = true;
        }
        var policy4 = await dbContext.Policies.FindAsync(4);
        if (policy4 != null && policy4.SourceUrl != "https://tuoitre.vn/ca-ngan-ban-tre-cung-ba-con-o-binh-hung-hoa-chung-tay-don-rac-trong-cay-nhan-chu-nhat-xanh-20260524125312066.htm")
        {
            policy4.SourceUrl = "https://tuoitre.vn/ca-ngan-ban-tre-cung-ba-con-o-binh-hung-hoa-chung-tay-don-rac-trong-cay-nhan-chu-nhat-xanh-20260524125312066.htm";
            hasChanges = true;
        }
        var policy5 = await dbContext.Policies.FindAsync(5);
        if (policy5 != null && policy5.SourceUrl != "https://tuoitre.vn/tp-hcm-chu-tich-phuong-xa-chiu-trach-nhiem-neu-via-he-bi-lan-chiem-20251128174818155.htm")
        {
            policy5.SourceUrl = "https://tuoitre.vn/tp-hcm-chu-tich-phuong-xa-chiu-trach-nhiem-neu-via-he-bi-lan-chiem-20251128174818155.htm";
            hasChanges = true;
        }
        var policy6 = await dbContext.Policies.FindAsync(6);
        if (policy6 != null && policy6.SourceUrl != "https://tuoitre.vn/khanh-hoa-trien-khai-ung-dung-de-dan-phan-anh-hien-truong-tra-cuu-dich-vu-cong-20260415093301147.htm")
        {
            policy6.SourceUrl = "https://tuoitre.vn/khanh-hoa-trien-khai-ung-dung-de-dan-phan-anh-hien-truong-tra-cuu-dich-vu-cong-20260415093301147.htm";
            hasChanges = true;
        }

        if (onlineUsers.Any() || hasChanges)
        {
            await dbContext.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra khi chạy seeding dữ liệu tài khoản mẫu, reset trạng thái online hoặc cập nhật SourceUrl.");
    }
}

// Cáº¥u hÃ¬nh HTTP request pipeline.
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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<DonationHub>("/hubs/donation");

app.Run();

public partial class Program { }


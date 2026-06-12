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

// Cáº¥u hÃ¬nh Database Connection
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

// ÄÄƒng kÃ½ cÃ¡c Repository vÃ  Service chuyÃªn biá»‡t
builder.Services.Configure<CivicConnect.Web.Models.CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IIssueRepository, IssueRepository>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IMomoService, MomoService>();

// ÄÄƒng kÃ½ cÃ¡c Hosted Services (Background Jobs cháº¡y ná»n)
builder.Services.AddHostedService<PriorityScoreJob>();
builder.Services.AddHostedService<DeadlineCheckJob>();

// ÄÄƒng kÃ½ SignalR Ä‘á»ƒ gá»­i thÃ´ng bÃ¡o realtime
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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lá»—i xáº£y ra khi cháº¡y seeding dá»¯ liá»‡u tÃ i khoáº£n máº«u.");
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


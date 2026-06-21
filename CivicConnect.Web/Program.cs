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

// Cấu hình MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Antiforgery to support header-based verification for JSON/AJAX POST requests
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

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
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Cấu hình External Authentication (Google)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
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

        // Self-healing database seeding and update for policies 1 to 6
        bool hasChanges = false;
        var requiredPolicies = new List<Policy>
        {
            new Policy
            {
                Id = 1,
                Title = "Nghị định 45/2026/NĐ-CP về xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị",
                Excerpt = "Chính phủ ban hành quy định tăng mức phạt đối với các hành vi xả rác bừa bãi, đổ chất thải không đúng nơi quy định và lấn chiếm lòng lề đường.",
                Content = @"<p><strong>NGHỊ ĐỊNH</strong><br/>
                          <strong>Quy định xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị</strong></p>

                          <p>Căn cứ Luật Tổ chức Chính phủ ngày 19 tháng 6 năm 2015; Luật sửa đổi, bổ sung một số điều của Luật Tổ chức Chính phủ và Luật Tổ chức chính quyền địa phương ngày 22 tháng 11 năm 2019;<br/>
                          Căn cứ Luật Bảo vệ môi trường ngày 17 tháng 11 năm 2020;<br/>
                          Căn cứ Luật Xử lý vi phạm hành chính ngày 20 tháng 6 năm 2012; Luật sửa đổi, bổ sung một số điều của Luật Xử lý vi phạm hành chính ngày 13 tháng 11 năm 2020;<br/>
                          Theo đề nghị của Bộ trưởng Bộ Tài nguyên và Môi trường;<br/>
                          Chính phủ ban hành Nghị định quy định xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị.</p>

                          <p><strong>Chương I: QUY ĐỊNH CHUNG</strong></p>

                          <p><strong>Điều 1. Phạm vi điều chỉnh</strong><br/>
                          Nghị định này quy định các hành vi vi phạm hành chính, hình thức xử phạt, mức xử phạt, biện pháp khắc phục hậu quả đối với hành vi vi phạm pháp luật về bảo vệ môi trường tại khu vực đô thị, khu dân cư tập trung, nơi công cộng.</p>

                          <p><strong>Điều 2. Đối tượng áp dụng</strong><br/>
                          1. Cá nhân, tổ chức trong nước và nước ngoài có hành vi vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị trên lãnh thổ nước Cộng hòa xã hội chủ nghĩa Việt Nam.<br/>
                          2. Cơ quan có thẩm quyền xử phạt và cá nhân, tổ chức liên quan.</p>

                          <p><strong>Chương II: HÀNH VI VI PHẠM, HÌNH THỨC VÀ MỨC XỬ PHẠT</strong></p>

                          <p><strong>Điều 10. Vi phạm quy định về bảo vệ môi trường nơi công cộng, khu đô thị, khu dân cư</strong><br/>
                          1. Phạt tiền từ 1.000.000 đồng đến 2.000.000 đồng đối với hành vi vứt, thải, bỏ đầu, mẩu, tàn thuốc lá không đúng nơi quy định tại khu chung cư, thương mại, dịch vụ hoặc nơi công cộng.<br/>
                          2. Phạt tiền từ 3.000.000 đồng đến 5.000.000 đồng đối với hành vi vệ sinh cá nhân (tiểu tiện, đại tiện) không đúng nơi quy định tại khu chung cư, thương mại, dịch vụ hoặc nơi công cộng.<br/>
                          3. Phạt tiền từ 5.000.000 đồng đến 10.000.000 đồng đối với hành vi vứt, thải, bỏ rác thải sinh hoạt, đổ nước thải không đúng nơi quy định tại khu chung cư, thương mại, dịch vụ hoặc nơi công cộng, trừ các hành vi quy định tại khoản 4 Điều này.<br/>
                          4. Phạt tiền từ 10.000.000 đồng đến 15.000.000 đồng đối với hành vi vứt, thải rác thải sinh hoạt trên vỉa hè, lòng đường hoặc vào hệ thống thoát nước mưa, nước thải đô thị; đổ nước thải không đúng quy định trên vỉa hè, lòng đường phố.</p>

                          <p><strong>Điều 11. Vi phạm về lấn chiếm lòng lề đường, xả rác thải xây dựng</strong><br/>
                          1. Phạt tiền từ 20.000.000 đồng đến 30.000.000 đồng đối với hành vi đổ, bỏ chất thải rắn xây dựng, đất đá, phế liệu xây dựng trái phép ra môi trường hoặc lấn chiếm lòng lề đường, hè phố đô thị.<br/>
                          2. Biện pháp khắc phục hậu quả: Buộc khôi phục lại tình trạng ban đầu; buộc vận chuyển chất thải, phế liệu xây dựng đến điểm tập kết đúng quy định.</p>

                          <p><strong>Chương III: THẨM QUYỀN VÀ THỦ TỤC XỬ PHẠT</strong></p>

                          <p><strong>Điều 25. Thẩm quyền của Chủ tịch Ủy ban nhân dân các cấp</strong><br/>
                          1. Chủ tịch Ủy ban nhân dân cấp xã có quyền phạt cảnh cáo, phạt tiền đến 5.000.000 đồng, tịch thu tang vật vi phạm.<br/>
                          2. Chủ tịch Ủy ban nhân dân cấp huyện có quyền phạt tiền đến 50.000.000 đồng, đình chỉ hoạt động gây ô nhiễm môi trường.<br/>
                          3. Chủ tịch Ủy ban nhân dân cấp tỉnh có quyền phạt tiền đến 100.000.000 đồng đối với cá nhân và 200.000.000 đồng đối với tổ chức.</p>",
                Tag = "Nghị định",
                TagClass = "tag-law",
                IssuingUnit = "Chính phủ",
                PublishedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "45/2026/NĐ-CP",
                DocumentType = "Nghị định",
                Signer = "Thủ tướng Phạm Minh Chính",
                SourceUrl = "https://tuoitre.vn/vut-rac-bua-bai-tieu-bay-se-bi-phat-gap-10-lan-1259236.htm",
                EffectiveDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 2,
                Title = "Thông tư 08/2026/TT-BXD hướng dẫn về chỉnh trang đô thị và phát triển không gian công cộng xanh",
                Excerpt = "Bộ Xây dựng ban hành hướng dẫn thực hiện các chỉ tiêu về diện tích cây xanh, hoa và hạ tầng tiện ích tại khu dân cư đô thị.",
                Content = @"<p><strong>BỘ XÂY DỰNG</strong><br/>
                          Số: 08/2026/TT-BXD<br/>
                          <em>Hà Nội, ngày 01 tháng 06 năm 2026</em></p>

                          <p><strong>THÔNG TƯ</strong><br/>
                          <strong>Hướng dẫn về chỉnh trang đô thị, cải tạo và đồng bộ hóa không gian công cộng xanh</strong></p>

                          <p>Căn cứ Luật Xây dựng ngày 18 tháng 6 năm 2014 và Luật sửa đổi, bổ sung một số điều của Luật Xây dựng ngày 17 tháng 6 năm 2020;<br/>
                          Căn cứ Luật Quy hoạch đô thị ngày 17 tháng 6 năm 2009;<br/>
                          Nhằm nâng cao chất lượng hạ tầng xanh và phát triển môi trường sống trong lành tại các đô thị loại I và đặc biệt;<br/>
                          Bộ trưởng Bộ Xây dựng ban hành Thông tư hướng dẫn về chỉnh trang đô thị, cải tạo và phát triển không gian công cộng xanh.</p>

                          <p><strong>Điều 1. Phạm vi áp dụng và tiêu chuẩn xanh</strong><br/>
                          1. Tiêu chuẩn diện tích cây xanh: Đề xuất các khu đô thị mới phải đạt tối thiểu 2m² cây xanh/người dân. Các khu dân cư hiện hữu tận dụng tối đa vỉa hè, các tuyến hẻm để trồng hoa, cây xanh công cộng tự quản.<br/>
                          2. Hạ tầng kỹ thuật vỉa hè: Khuyến khích cải tạo, chỉnh trang đồng bộ bằng đá tự nhiên có độ bền cao, thiết lập các gờ nổi dẫn đường cho người khiếm thị.</p>

                          <p><strong>Điều 2. Quy chuẩn cải tạo và nguồn lực hỗ trợ</strong><br/>
                          1. Ưu tiên ngân sách hỗ trợ 70% tổng kinh phí đầu tư xây dựng cơ sở hạ tầng xanh. Vận động nguồn lực xã hội đóng góp 30% từ các doanh nghiệp, hộ gia đình mặt tiền đường.<br/>
                          2. Hỗ trợ 100% kinh phí chỉnh trang kết nối từ nhà ra vỉa hè cho các hộ nghèo, cận nghèo và gia đình chính sách.</p>",
                Tag = "Thông tư",
                TagClass = "tag-policy",
                IssuingUnit = "Bộ Xây dựng",
                PublishedDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "08/2026/TT-BXD",
                DocumentType = "Thông tư",
                Signer = "Bộ trưởng Nguyễn Thanh Nghị",
                SourceUrl = "https://tuoitre.vn/de-xuat-xay-cong-vien-khoa-hoc-cho-thieu-nhi-tai-tp-hcm-20240906132157012.htm",
                EffectiveDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 3,
                Title = "Thông báo 124/TB-UBND ra quân tổng vệ sinh môi trường trên địa bàn Phường Bến Nghé",
                Excerpt = "UBND Phường phát động lễ ra quân tổng vệ sinh, dọn dẹp rác thải và trang trí tuyến hẻm xanh vào sáng Chủ Nhật.",
                Content = @"<p><strong>ỦY BAN NHÂN DÂN PHƯỜNG BẾN NGHÉ</strong><br/>
                          Số: 124/TB-UBND<br/>
                          <em>Bến Nghé, ngày 10 tháng 06 năm 2026</em></p>

                          <p><strong>THÔNG BÁO</strong><br/>
                          <strong>Về việc tổ chức ra quân tổng vệ sinh môi trường, xóa quảng cáo rao vặt trái phép và chỉnh trang mỹ quan đô thị</strong></p>

                          <p>Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung trên địa bàn Phường, Ủy ban nhân dân Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ.</p>

                          <p><strong>1. Thời gian ra quân:</strong><br/>
                          Bắt đầu từ 07 giờ 30 phút sáng Chủ Nhật ngày 21 tháng 06 năm 2026.</p>

                          <p><strong>2. Phân công nhiệm vụ cụ thể:</strong><br/>
                          - Đoàn Thanh niên phường phối hợp dọn dẹp rác thải nhựa dọc các tuyến đường lớn như Nguyễn Huệ và Lê Lợi.<br/>
                          - Ban điều hành khu phố vận động từng hộ gia đình quét dọn sạch sẽ, phân loại rác thải tại nguồn trước cửa nhà mình.<br/>
                          - Lực lượng chức năng phối hợp bóc gỡ quảng cáo vẽ bậy trái phép trên tường, tủ điện công cộng.</p>",
                Tag = "Thông báo",
                TagClass = "tag-notice",
                IssuingUnit = "UBND Phường Bến Nghé",
                PublishedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "124/TB-UBND",
                DocumentType = "Thông báo",
                Signer = "Chủ tịch UBND Phường",
                SourceUrl = "",
                EffectiveDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 4,
                Title = "Hơn 500 đoàn viên thanh niên Quận 1 tham gia làm sạch kênh Nhiêu Lộc - Thị Nghè",
                Excerpt = "Chiến dịch 'Chung tay bảo vệ dòng sông quê hương' thu hút hàng trăm bạn trẻ dọn dẹp rác thải nhựa và vớt lục bình.",
                Content = "Sáng nay, Quận Đoàn 1 phối hợp với Công ty Môi trường Đô thị tổ chức lễ phát động ra quân làm sạch toàn tuyến kênh chảy qua địa bàn quận. Sau 4 giờ làm việc nỗ lực, lực lượng tình nguyện đã thu gom được hơn 3 tấn rác thải các loại, chủ yếu là túi ni lông, chai nhựa và rác sinh hoạt bị vứt xuống lòng kênh. Đây là hoạt động thường niên nhằm tuyên truyền ý thức bảo vệ môi trường nước cho cư dân xung quanh.",
                Tag = "Tin tức",
                TagClass = "tag-news",
                IssuingUnit = "Quận Đoàn 1",
                PublishedDate = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "",
                DocumentType = "Tin tức",
                Signer = "",
                SourceUrl = "https://tuoitre.vn/ca-ngan-ban-tre-cung-ba-con-o-binh-hung-hoa-chung-tay-don-rac-trong-cay-nhan-chu-nhat-xanh-20260524125312066.htm",
                EffectiveDate = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 5,
                Title = "Quyết định 18/2026/QĐ-UBND về quản lý và sử dụng tạm thời một phần lòng đường, vỉa hè tại TP.HCM",
                Excerpt = "UBND Thành phố Hồ Chí Minh ban hành quy định mới về quản lý, cấp phép và thu phí sử dụng tạm thời một phần lòng đường, hè phố không vì mục đích giao thông.",
                Content = @"<p><strong>ỦY BAN NHÂN DÂN THÀNH PHỐ HỒ CHÍ MINH</strong><br/>
                          Số: 18/2026/QĐ-UBND<br/>
                          <em>TP. Hồ Chí Minh, ngày 15 tháng 05 năm 2026</em></p>

                          <p><strong>QUYẾT ĐỊNH</strong><br/>
                          <strong>Ban hành Quy định về quản lý và sử dụng tạm thời một phần lòng đường, hè phố trên địa bàn Thành phố Hồ Chí Minh</strong></p>

                          <p>Căn cứ Luật Tổ chức chính quyền địa phương ngày 19 tháng 6 năm 2015;<br/>
                          Căn cứ Luật Giao thông đường bộ ngày 13 tháng 11 năm 2008;<br/>
                          Nhằm thiết lập trật tự, kỷ cương đô thị, đồng thời giải quyết nhu cầu sử dụng tạm thời hè phố của người dân và doanh nghiệp một cách công khai, minh bạch;<br/>
                          Theo đề nghị của Giám đốc Sở Giao thông vận tải Thành phố Hồ Chí Minh.</p>

                          <p><strong>Điều 1. Phạm vi và nguyên tắc sử dụng tạm thời hè phố</strong><br/>
                          1. Hè phố chỉ được sử dụng tạm thời cho mục đích ngoài giao thông khi phần hè phố còn lại dành cho người đi bộ có bề rộng tối thiểu là 1,5 mét, thông suốt và an toàn.<br/>
                          2. Việc sử dụng tạm thời phải được cấp phép bởi cơ quan có thẩm quyền và phải đóng phí sử dụng đường bộ theo quy định.</p>

                          <p><strong>Điều 2. Các trường hợp được sử dụng tạm thời đóng phí</strong><br/>
                          1. Điểm tổ chức kinh doanh dịch vụ mua, bán hàng hóa, ẩm thực tại các tuyến phố đi bộ hoặc khu vực được quy hoạch.<br/>
                          2. Điểm trông giữ xe đạp, xe máy, xe ô tô có thu tiền dịch vụ.<br/>
                          3. Tổ chức các hoạt động văn hóa, xã hội, tuyên truyền cổ động lớn của Thành phố hoặc Quận/Huyện.</p>

                          <p><strong>Điều 3. Thẩm quyền cấp phép và mức phí</strong><br/>
                          1. Ủy ban nhân dân các Quận, Huyện và Thành phố Thủ Đức thực hiện cấp phép sử dụng tạm thời hè phố thuộc phạm vi quản lý hành chính.<br/>
                          2. Mức phí được áp dụng theo biểu giá phân chia theo 5 khu vực đô thị của Thành phố, dao động từ 20.000 đồng đến 350.000 đồng/m²/tháng đối với kinh doanh hàng hóa và trông giữ xe.</p>",
                Tag = "Quyết định",
                TagClass = "tag-law",
                IssuingUnit = "UBND TP.HCM",
                PublishedDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "18/2026/QĐ-UBND",
                DocumentType = "Quyết định",
                Signer = "Chủ tịch Phan Văn Mãi",
                SourceUrl = "https://tuoitre.vn/tp-hcm-chu-tich-phuong-xa-chiu-trach-nhiem-neu-via-he-bi-lan-chiem-20251128174818155.htm",
                EffectiveDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 6,
                Title = "Kế hoạch 89/KH-UBND nâng cao hiệu quả tiếp nhận và giải quyết phản ánh, kiến nghị của người dân đô thị",
                Excerpt = "UBND Quận 1 ban hành kế hoạch hành động nhằm tối ưu hóa quy trình tiếp nhận phản ánh về trật tự đô thị, vệ sinh môi trường và hạ tầng kỹ thuật.",
                Content = @"<p><strong>ỦY BAN NHÂN DÂN QUẬN 1</strong><br/>
                          Số: 89/KH-UBND<br/>
                          <em>Quận 1, ngày 25 tháng 05 năm 2026</em></p>

                          <p><strong>KẾ HOẠCH</strong><br/>
                          <strong>Nâng cao hiệu quả công tác tiếp nhận, xử lý và trả lời phản ánh, kiến nghị của người dân qua Hệ thống tương tác chính quyền số</strong></p>

                          <p>Nhằm tăng cường sự hài lòng của người dân, rút ngắn thời gian xử lý các sự cố về hạ tầng đô thị, an ninh trật tự và vệ sinh môi trường trên địa bàn Quận 1;<br/>
                          Ủy ban nhân dân Quận 1 ban hành Kế hoạch hành động cụ thể cho giai đoạn 2026 - 2027.</p>

                          <p><strong>1. Chỉ tiêu xử lý phản ánh kiến nghị</strong><br/>
                          - 100% phản ánh của người dân về các sự cố khẩn cấp (như sụt lún đường, đứt cáp điện, ô nhiễm nghiêm trọng) phải được tiếp nhận và xử lý ban đầu trong vòng 2 giờ.<br/>
                          - Ít nhất 95% phản ánh thông thường (như rác thải sinh hoạt, lấn chiếm hè phố, tiếng ồn khu dân cư) phải được giải quyết dứt điểm và phản hồi kết quả cho công dân trong vòng 24 giờ kể từ khi tiếp nhận.</p>

                          <p><strong>2. Phân công trách nhiệm</strong><br/>
                          - Ủy ban nhân dân 10 phường trực thuộc chịu trách nhiệm xử lý trực tiếp tại hiện trường đối với các phản ánh về trật tự đô thị, vệ sinh môi trường trên địa bàn.<br/>
                          - Phòng Quản lý đô thị phối hợp cùng Phòng Tài nguyên và Môi trường Quận giám sát, đôn đốc tiến độ xử lý và hậu kiểm kết quả tại các đơn vị cơ sở.</p>

                          <p><strong>3. Khen thưởng và kỷ luật</strong><br/>
                          - Đưa chỉ tiêu tỷ lệ giải quyết đúng hạn các phản ánh của công dân làm tiêu chí xếp loại thi đua hàng năm của các đơn vị phường, phòng ban và cá nhân người đứng đầu.<br/>
                          - Xử lý nghiêm khắc đối với các trường hợp trễ hẹn không có lý do chính đáng hoặc trả lời phản ánh mang tính đối phó, không dứt điểm.</p>",
                Tag = "Kế hoạch",
                TagClass = "tag-policy",
                IssuingUnit = "UBND Quận 1",
                PublishedDate = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "89/KH-UBND",
                DocumentType = "Kế hoạch",
                Signer = "Chủ tịch UBND Quận 1",
                SourceUrl = "https://tuoitre.vn/khanh-hoa-trien-khai-ung-dung-de-dan-phan-anh-hien-truong-tra-cuu-dich-vu-cong-20260415093301147.htm",
                EffectiveDate = new DateTime(2026, 5, 25, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 7,
                Title = "Luật số 10/2022/QH15 về thực hiện dân chủ ở cơ sở",
                Excerpt = "Quốc hội ban hành luật quy định về quyền và nghĩa vụ của công dân trong việc giám sát, kiểm tra và phản ánh ý kiến đến chính quyền địa phương.",
                Content = @"<p><strong>LUẬT</strong><br/>
                          <strong>THỰC HIỆN DÂN CHỦ Ở CƠ SỞ</strong></p>

                          <p>Căn cứ Hiến pháp nước Cộng hòa xã hội chủ nghĩa Việt Nam;<br/>
                          Quốc hội ban hành Luật Thực hiện dân chủ ở cơ sở.</p>

                          <p><strong>Chương I: QUY ĐỊNH CHUNG</strong></p>

                          <p><strong>Điều 5. Quyền của công dân trong thực hiện dân chủ ở cơ sở</strong><br/>
                          1. Được thụ hưởng thông tin công khai từ cơ quan nhà nước đầy đủ và minh bạch.<br/>
                          2. Đề xuất, kiến nghị, phản ánh, thảo luận và tham gia đóng góp ý kiến về các nội dung liên quan trực tiếp đến đời sống xã hội và hạ tầng đô thị tại cơ sở.<br/>
                          3. Thực hiện quyền kiểm tra, giám sát hoạt động của chính quyền cấp cơ sở theo đúng quy định pháp luật.</p>

                          <p><strong>Điều 6. Nghĩa vụ của công dân</strong><br/>
                          1. Tuân thủ Hiến pháp và pháp luật, tôn trọng trật tự văn minh đô thị.<br/>
                          2. Phối hợp cùng cơ quan chức năng phản ánh kịp thời các hành vi vi phạm trật tự công cộng, gây ô nhiễm môi trường hoặc hư hỏng hạ tầng kỹ thuật.</p>",
                Tag = "Luật",
                TagClass = "tag-law",
                IssuingUnit = "Quốc hội",
                PublishedDate = new DateTime(2022, 11, 10, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "10/2022/QH15",
                DocumentType = "Luật",
                Signer = "Chủ tịch Quốc hội Vương Đình Huệ",
                SourceUrl = "https://chinhphu.vn/",
                EffectiveDate = new DateTime(2023, 7, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 8,
                Title = "Nghị định 59/2022/NĐ-CP của Chính phủ quy định về định danh và xác thực điện tử qua VNeID",
                Excerpt = "Chính phủ quy định về danh tính điện tử, xác thực điện tử và việc sử dụng tài khoản VNeID thay thế giấy tờ vật lý.",
                Content = @"<p><strong>NGHỊ ĐỊNH</strong><br/>
                          <strong>Quy định về định danh và xác thực điện tử</strong></p>

                          <p>Căn cứ Luật Tổ chức Chính phủ ngày 19 tháng 6 năm 2015;<br/>
                          Căn cứ Luật An toàn thông tin mạng ngày 19 tháng 11 năm 2015;<br/>
                          Theo đề nghị của Bộ trưởng Bộ Công an;<br/>
                          Chính phủ ban hành Nghị định quy định về định danh và xác thực điện tử.</p>

                          <p><strong>Điều 6. Phân loại mức độ tài khoản định danh điện tử</strong><br/>
                          1. Mức độ 1: Xác thực qua thông tin cá nhân và ảnh chân dung đối chiếu với Cơ sở dữ liệu quốc gia về dân cư.<br/>
                          2. Mức độ 2: Xác thực mức độ cao nhất tích hợp sinh trắc học vân tay hoặc chíp điện tử CCCD, có giá trị tương đương thẻ căn cước công dân vật lý trong thực hiện thủ tục hành chính.</p>",
                Tag = "Nghị định",
                TagClass = "tag-law",
                IssuingUnit = "Chính phủ",
                PublishedDate = new DateTime(2022, 9, 5, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "59/2022/NĐ-CP",
                DocumentType = "Nghị định",
                Signer = "Thủ tướng Phạm Minh Chính",
                SourceUrl = "https://chinhphu.vn/",
                EffectiveDate = new DateTime(2022, 10, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 9,
                Title = "Luật số 31/2024/QH15 về Đất đai (Luật Đất đai năm 2024)",
                Excerpt = "Quốc hội ban hành Luật Đất đai sửa đổi toàn diện quy định về chế độ sở hữu đất đai, quyền hạn và trách nhiệm của Nhà nước đại diện chủ sở hữu, quy định về bồi thường, hỗ trợ, tái định cư khi Nhà nước thu hồi đất.",
                Content = @"<p><strong>LUẬT ĐẤT ĐAI (NĂM 2024)</strong><br/>
                          Số: 31/2024/QH15<br/>
                          <em>Hà Nội, ngày 18 tháng 01 năm 2024</em></p>

                          <p>Căn cứ Hiến pháp nước Cộng hòa xã hội chủ nghĩa Việt Nam;<br/>
                          Quốc hội ban hành Luật Đất đai.</p>

                          <p><strong>Chương VI: THU HỒI ĐẤT, TRƯNG DỤNG ĐẤT</strong></p>

                          <p><strong>Điều 79. Thu hồi đất để phát triển kinh tế - xã hội vì lợi ích quốc gia, công cộng</strong><br/>
                          Nhà nước thu hồi đất trong trường hợp thật cần thiết để thực hiện dự án phát triển kinh tế - xã hội vì lợi ích quốc gia, công cộng nhằm phát huy nguồn lực đất đai, nâng cao hiệu quả sử dụng đất, phát triển hạ tầng kỹ thuật, hạ tầng xã hội.</p>

                          <p><strong>Chương VII: BỒI THƯỜNG, HỖ TRỢ, TÁI ĐỊNH CƯ KHI NHÀ NƯỚC THU HỒI ĐẤT</strong></p>

                          <p><strong>Điều 91. Nguyên tắc bồi thường, hỗ trợ, tái định cư khi Nhà nước thu hồi đất</strong><br/>
                          1. Việc bồi thường về đất được thực hiện bằng việc giao đất có cùng mục đích sử dụng với loại đất thu hồi; nếu không có đất để bồi thường thì được bồi thường bằng tiền theo giá đất cụ thể của loại đất thu hồi do Ủy ban nhân dân cấp có thẩm quyền quyết định tại thời điểm phê duyệt phương án bồi thường.<br/>
                          2. Việc bồi thường khi Nhà nước thu hồi đất phải bảo đảm dân chủ, khách quan, công bằng, công khai, kịp thời và đúng quy định của pháp luật; bảo đảm người có đất bị thu hồi có chỗ ở, ổn định đời sống, sản xuất.<br/>
                          3. Khu tái định cư phải hoàn thiện hệ thống hạ tầng kỹ thuật, hạ tầng xã hội đồng bộ trước khi phê duyệt phương án bồi thường, hỗ trợ, tái định cư.</p>

                          <p><strong>Điều 110. Hỗ trợ tái định cư khi Nhà nước thu hồi đất ở</strong><br/>
                          Hộ gia đình, cá nhân bị thu hồi đất ở được hỗ trợ tái định cư bằng nhà ở tái định cư hoặc đất ở tái định cư. Trường hợp hộ gia đình, cá nhân tự lo chỗ ở thì ngoài việc được bồi thường về đất còn được hỗ trợ một khoản tiền tự lo chỗ ở.</p>",
                Tag = "Luật",
                TagClass = "tag-law",
                IssuingUnit = "Quốc hội",
                PublishedDate = new DateTime(2024, 1, 18, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "31/2024/QH15",
                DocumentType = "Luật",
                Signer = "Chủ tịch Quốc hội Vương Đình Huệ",
                SourceUrl = "https://chinhphu.vn/",
                EffectiveDate = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 10,
                Title = "Nghị định 100/2019/NĐ-CP của Chính phủ về xử phạt vi phạm hành chính trong lĩnh vực giao thông đường bộ",
                Excerpt = "Chính phủ ban hành quy định chi tiết về xử phạt hành vi vi phạm trật tự, an toàn giao thông, trong đó có mức phạt nghiêm khắc đối với nồng độ cồn và không đội mũ bảo hiểm.",
                Content = @"<p><strong>NGHỊ ĐỊNH</strong><br/>
                          <strong>Quy định xử phạt vi phạm hành chính trong lĩnh vực giao thông đường bộ và đường sắt</strong></p>

                          <p>Căn cứ Luật Tổ chức Chính phủ ngày 19 tháng 6 năm 2015;<br/>
                          Căn cứ Luật Xử lý vi phạm hành chính ngày 20 tháng 6 năm 2012;<br/>
                          Căn cứ Luật Giao thông đường bộ ngày 13 tháng 11 năm 2008;<br/>
                          Chính phủ ban hành Nghị định quy định xử phạt vi phạm hành chính trong lĩnh vực giao thông đường bộ và đường sắt.</p>

                          <p><strong>Điều 5. Xử phạt người điều khiển xe ô tô vi phạm quy định về giao thông đường bộ</strong><br/>
                          1. Phạt tiền từ 6.000.000 đồng đến 8.000.000 đồng đối với người điều khiển xe trên đường mà trong máu hoặc hơi thở có nồng độ cồn nhưng chưa vượt quá 50 miligam/100 mililít máu hoặc chưa vượt quá 0,25 miligam/1 lít khí thở.<br/>
                          2. Phạt tiền từ 16.000.000 đồng đến 18.000.000 đồng đối với nồng độ cồn vượt quá 50 miligam đến 80 miligam/100 mililít máu.<br/>
                          3. Phạt tiền từ 30.000.000 đồng đến 40.000.000 đồng đối với nồng độ cồn vượt quá 80 miligam/100 mililít máu hoặc vượt quá 0,4 miligam/1 lít khí thở, kèm theo tước quyền sử dụng Giấy phép lái xe từ 22 tháng đến 24 tháng.</p>

                          <p><strong>Điều 6. Xử phạt người điều khiển xe mô tô, xe gắn máy vi phạm</strong><br/>
                          1. Phạt tiền từ 2.000.000 đồng đến 3.000.000 đồng đối với hành vi điều khiển xe trên đường mà trong hơi thở có nồng độ cồn chưa vượt quá 0,25 miligam/1 lít khí thở.<br/>
                          2. Phạt tiền từ 4.000.000 đồng đến 5.000.000 đồng đối với nồng độ cồn vượt quá 0,25 đến 0,4 miligam/1 lít khí thở.<br/>
                          3. Phạt tiền từ 6.000.000 đồng đến 8.000.000 đồng đối với nồng độ cồn vượt quá 0,4 miligam/1 lít khí thở, tước Giấy phép lái xe từ 22 đến 24 tháng.</p>

                          <p><strong>Điều 30. Xử phạt chủ phương tiện vi phạm quy định liên quan đến giao thông đường bộ</strong><br/>
                          Phạt tiền từ 400.000 đồng đến 600.000 đồng đối với người điều khiển xe mô tô, xe gắn máy và các loại xe tương tự không đội mũ bảo hiểm cho người đi mô tô, xe máy hoặc đội mũ bảo hiểm không cài quai đúng quy cách khi tham gia giao thông.</p>",
                Tag = "Nghị định",
                TagClass = "tag-law",
                IssuingUnit = "Chính phủ",
                PublishedDate = new DateTime(2019, 12, 30, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "100/2019/NĐ-CP",
                DocumentType = "Nghị định",
                Signer = "Thủ tướng Nguyễn Xuân Phúc",
                SourceUrl = "https://chinhphu.vn/",
                EffectiveDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Policy
            {
                Id = 11,
                Title = "Nghị định 144/2021/NĐ-CP xử phạt vi phạm hành chính về an ninh, trật tự xã hội và tiếng ồn khu dân cư",
                Excerpt = "Quy định chi tiết các mức xử phạt đối với các hành vi gây mất trật tự công cộng, đặc biệt là hát karaoke lớn tiếng, loa kéo gây ồn ào sau 22h đêm trong khu dân cư đô thị.",
                Content = @"<p><strong>NGHỊ ĐỊNH</strong><br/>
                          <strong>Quy định xử phạt vi phạm hành chính trong lĩnh vực an ninh, trật tự, an toàn xã hội; phòng, chống tệ nạn xã hội; phòng cháy, chữa cháy; cứu nạn, cứu hộ; phòng, chống bạo lực gia đình</strong></p>

                          <p>Căn cứ Luật Tổ chức Chính phủ ngày 19 tháng 6 năm 2015;<br/>
                          Căn cứ Luật Xử lý vi phạm hành chính ngày 20 tháng 6 năm 2012;<br/>
                          Theo đề nghị của Bộ trưởng Bộ Công an;<br/>
                          Chính phủ ban hành Nghị định.</p>

                          <p><strong>Điều 8. Vi phạm quy định về bảo đảm sự yên tĩnh chung</strong><br/>
                          1. Phạt cảnh cáo hoặc phạt tiền từ 500.000 đồng đến 1.000.000 đồng đối với một trong các hành vi sau đây thực hiện trong khoảng thời gian từ 22 giờ ngày hôm trước đến 06 giờ sáng ngày hôm sau:<br/>
                          a) Gây tiếng động lớn, làm ồn ào, huyên náo tại khu dân cư, nơi công cộng;<br/>
                          b) Dùng loa phóng thanh, loa kéo, thiết bị âm thanh công suất lớn phát nhạc, hát karaoke gây ảnh hưởng đến sự yên tĩnh chung của khu dân cư.<br/>
                          2. Tịch thu tang vật, phương tiện vi phạm hành chính đối với hành vi quy định tại khoản 1 Điều này nếu tái phạm nhiều lần.</p>

                          <p><strong>Điều 7. Vi phạm quy định về nếp sống văn minh, trật tự công cộng</strong><br/>
                          1. Phạt cảnh cáo hoặc phạt tiền từ 300.000 đồng đến 500.000 đồng đối với hành vi thả rông động vật nuôi nơi đô thị hoặc nơi công cộng mà không có rọ mõm hoặc không có người dắt.<br/>
                          2. Phạt tiền từ 1.000.000 đồng đến 2.000.000 đồng đối với hành vi phun sơn, vẽ, viết, treo, dán tranh, ảnh, quảng cáo lên tường, cột điện, công trình công cộng mà không được phép của cơ quan có thẩm quyền.</p>",
                Tag = "Nghị định",
                TagClass = "tag-law",
                IssuingUnit = "Chính phủ",
                PublishedDate = new DateTime(2021, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true,
                DocumentNumber = "144/2021/NĐ-CP",
                DocumentType = "Nghị định",
                Signer = "Thủ tướng Phạm Minh Chính",
                SourceUrl = "https://chinhphu.vn/",
                EffectiveDate = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        foreach (var reqPolicy in requiredPolicies)
        {
            var existing = await dbContext.Policies.FindAsync(reqPolicy.Id);
            if (existing == null)
            {
                bool isSqlServer = dbContext.Database.IsSqlServer();
                if (isSqlServer)
                {
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Policies ON");
                            await dbContext.Policies.AddAsync(reqPolicy);
                            await dbContext.SaveChangesAsync();
                            await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Policies OFF");
                            await transaction.CommitAsync();
                            hasChanges = true;
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
                else
                {
                    await dbContext.Policies.AddAsync(reqPolicy);
                    hasChanges = true;
                }
            }
            else
            {
                if (existing.SourceUrl != reqPolicy.SourceUrl || existing.Title != reqPolicy.Title)
                {
                    existing.Title = reqPolicy.Title;
                    existing.Excerpt = reqPolicy.Excerpt;
                    existing.Content = reqPolicy.Content;
                    existing.Tag = reqPolicy.Tag;
                    existing.TagClass = reqPolicy.TagClass;
                    existing.IssuingUnit = reqPolicy.IssuingUnit;
                    existing.DocumentNumber = reqPolicy.DocumentNumber;
                    existing.DocumentType = reqPolicy.DocumentType;
                    existing.Signer = reqPolicy.Signer;
                    existing.SourceUrl = reqPolicy.SourceUrl;
                    existing.EffectiveDate = reqPolicy.EffectiveDate;
                    hasChanges = true;
                }
            }
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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "short_issues",
    pattern: "Issues/{id:int}",
    defaults: new { controller = "Issues", action = "Details" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<DonationHub>("/hubs/donation");

app.Run();

public partial class Program { }


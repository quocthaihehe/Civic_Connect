using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContentController : Controller
    {
        private readonly AppDbContext _context;

        public ContentController(AppDbContext context)
        {
            _context = context;
        }

        // --- B.5.1 Trình soạn thảo Tin tức & Chính sách ---
        [HttpGet]
        [Route("Admin/Policies")]
        public async Task<IActionResult> Index()
        {
            var policies = await _context.Policies
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();
            return View("~/Areas/Admin/Views/Policies/Index.cshtml", policies);
        }

        [HttpPost]
        [Route("Admin/Policies/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePolicy(string title, string excerpt, string content, string tag, string issuingUnit, string? documentNumber, string? signer, string? sourceUrl, DateTime? effectiveDate)
        {
            var policy = new Policy
            {
                Title = title,
                Excerpt = excerpt,
                Content = content,
                Tag = tag,
                TagClass = tag == "Thông báo" ? "tag-notice" : "tag-news",
                IssuingUnit = issuingUnit,
                PublishedDate = DateTime.UtcNow,
                IsActive = true,
                DocumentNumber = documentNumber,
                Signer = signer,
                SourceUrl = sourceUrl,
                EffectiveDate = effectiveDate
            };

            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã đăng tải tin tức / chính sách mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        // --- B.5.2 Thông báo đẩy (Bulk Notifications) ---
        [HttpGet]
        [Route("Admin/Content/Notifications")]
        public IActionResult Notifications()
        {
            return View();
        }

        [HttpPost]
        [Route("Admin/Content/SendBulk")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBulk(string channel, string targetSegment, string title, string message)
        {
            // Simulate bulk sending
            // Create a notification record in CSDL
            var users = await _context.Users.Where(u => u.GovernmentUnitId == null).ToListAsync();
            
            foreach (var user in users)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = title,
                    Message = $"[{channel.ToUpper()}] {message}",
                    Type = NotificationType.General,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã gửi hàng loạt thông báo qua kênh {channel.ToUpper()} tới {users.Count} công dân phân khúc {targetSegment} thành công!";
            return RedirectToAction(nameof(Notifications));
        }

        // --- B.5.5 Duyệt bài viết Cộng đồng ---
        [HttpGet]
        [Route("Admin/Content/PendingPosts")]
        public async Task<IActionResult> PendingPosts()
        {
            var posts = await _context.ForumPosts
                .Include(p => p.Author)
                .Where(p => p.Status == PostStatus.Pending)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        [HttpPost]
        [Route("Admin/Content/ApprovePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null && post.Status == PostStatus.Pending)
            {
                post.Status = PostStatus.Approved;
                
                // Thưởng điểm
                var pointsService = HttpContext.RequestServices.GetService(typeof(CivicConnect.Web.Services.ICitizenPointsService)) as CivicConnect.Web.Services.ICitizenPointsService;
                if (pointsService != null)
                {
                    await pointsService.AwardPointsAsync(post.AuthorId, 10, 0, $"Bài viết '{post.Title}' được duyệt");
                }
                
                // Gửi thông báo cho User (SignalR)
                var hubContext = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<CivicConnect.Web.Hubs.NotificationHub>)) as Microsoft.AspNetCore.SignalR.IHubContext<CivicConnect.Web.Hubs.NotificationHub>;
                if (hubContext != null)
                {
                    var notif = new Notification
                    {
                        UserId = post.AuthorId,
                        Title = "Bài viết được duyệt",
                        Message = $"✅ Bài viết '{post.Title}' của bạn đã được phê duyệt và xuất hiện trên bảng tin!",
                        Type = NotificationType.General,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notif);
                    await hubContext.Clients.User(post.AuthorId).SendAsync("ReceiveNotification", notif);
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã duyệt bài viết thành công.";
            }
            return RedirectToAction(nameof(PendingPosts));
        }

        [HttpPost]
        [Route("Admin/Content/RejectPost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPost(int id, string reason)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post != null && post.Status == PostStatus.Pending)
            {
                post.Status = PostStatus.Rejected;
                post.RejectionReason = reason;
                
                // Gửi thông báo cho User (SignalR)
                var hubContext = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<CivicConnect.Web.Hubs.NotificationHub>)) as Microsoft.AspNetCore.SignalR.IHubContext<CivicConnect.Web.Hubs.NotificationHub>;
                if (hubContext != null)
                {
                    var notif = new Notification
                    {
                        UserId = post.AuthorId,
                        Title = "Bài viết bị từ chối",
                        Message = $"❌ Bài viết '{post.Title}' bị từ chối. Lý do: {reason}",
                        Type = NotificationType.General,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notif);
                    await hubContext.Clients.User(post.AuthorId).SendAsync("ReceiveNotification", notif);
                }
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã từ chối bài viết.";
            }
            return RedirectToAction(nameof(PendingPosts));
        }

        // --- B.5.4 Quản lý Danh mục & SLA & Từ khóa cấm ---
        [HttpGet]
        [Route("Admin/Content/Categories")]
        public async Task<IActionResult> Categories()
        {
            // Read settings for blacklist or default SLA
            var blacklist = await _context.SystemSettings.FindAsync("KeywordBlacklist");
            ViewBag.Blacklist = blacklist?.SettingValue ?? "phản động, thô tục, chửi thề";

            return View();
        }

        [HttpPost]
        [Route("Admin/Content/SaveCategories")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCategories(string blacklistKeywords)
        {
            var blacklist = await _context.SystemSettings.FindAsync("KeywordBlacklist");
            if (blacklist == null)
            {
                _context.SystemSettings.Add(new SystemSetting
                {
                    SettingKey = "KeywordBlacklist",
                    SettingValue = blacklistKeywords,
                    Description = "Danh sách từ khóa cấm viết cách nhau bằng dấu phẩy"
                });
            }
            else
            {
                blacklist.SettingValue = blacklistKeywords;
                blacklist.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật danh sách từ khóa cấm và phân cấu hình danh mục thành công!";
            return RedirectToAction(nameof(Categories));
        }
    }
}

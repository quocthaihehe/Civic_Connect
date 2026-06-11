using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Infrastructure.Data;
using CivicConnect.Core.Enums;
using CivicConnect.Core.Entities;
using CivicConnect.Web.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CivicConnect.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Challenge();

            // Redirect non-citizens
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            if (User.IsInRole("OfficialWard") || User.IsInRole("OfficialDistrict") || User.IsInRole("OfficialProvince") || User.IsInRole("DepartmentStaff"))
            {
                return RedirectToAction("Dashboard", "Official");
            }

            // Query pending count of user
            var myPendingCount = await _context.Issues
                .CountAsync(i => i.AuthorId == userId && i.Status == IssueStatus.Pending);

            // Query 3 latest policies
            var latestPolicies = await _context.Policies
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedDate)
                .Take(3)
                .Select(p => new PolicySummaryDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Excerpt = p.Excerpt,
                    Tag = p.Tag,
                    TagClass = p.TagClass,
                    IssuingUnit = p.IssuingUnit,
                    PublishedDate = p.PublishedDate
                })
                .ToListAsync();

            // Query 6 featured issues
            var featuredIssues = await _context.Issues
                .Include(i => i.Images)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                .Include(i => i.Author)
                .Where(i => i.Status == IssueStatus.Processing || i.Status == IssueStatus.Resolved)
                .OrderByDescending(i => i.Votes.Count)
                .Take(6)
                .Select(i => new IssueFeedDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    ThumbnailUrl = i.Images.OrderBy(img => img.OrderIndex).Select(img => img.Url).FirstOrDefault(),
                    Address = i.Address,
                    CategoryLabel = GetCategoryName(i.Category),
                    CategoryClass = GetCategoryClass(i.Category),
                    StatusLabel = GetStatusName(i.Status),
                    StatusClass = GetStatusClass(i.Status),
                    VoteCount = i.Votes.Count(v => v.Type == VoteType.Up),
                    CommentCount = i.Comments.Count,
                    AuthorName = i.IsAnonymous ? "Ẩn danh" : i.Author != null ? i.Author.FullName : "Công dân",
                    AuthorAvatar = i.IsAnonymous ? "/images/default-avatar.png" : i.Author != null && i.Author.AvatarUrl != null ? i.Author.AvatarUrl : "/images/default-avatar.png",
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            // Query 3 recent issues of user
            var myRecentIssues = await _context.Issues
                .Where(i => i.AuthorId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .Take(3)
                .Select(i => new MyIssueRowDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    CategoryLabel = GetCategoryName(i.Category),
                    StatusLabel = GetStatusName(i.Status),
                    StatusClass = GetStatusClass(i.Status),
                    Status = i.Status,
                    CreatedAt = i.CreatedAt,
                    DueDate = i.DueDate
                })
                .ToListAsync();

            // Resolve Ward Name and District Name
            var wardName = "khu vực của bạn";
            var districtName = "";
            if (!string.IsNullOrEmpty(user.WardCode))
            {
                var wardUnit = await _context.GovernmentUnits.FirstOrDefaultAsync(gu => gu.WardCode == user.WardCode);
                if (wardUnit != null) wardName = wardUnit.Name;
            }
            if (!string.IsNullOrEmpty(user.DistrictCode))
            {
                var districtUnit = await _context.GovernmentUnits.FirstOrDefaultAsync(gu => gu.DistrictCode == user.DistrictCode && gu.Type == GovernmentUnitType.District);
                if (districtUnit != null) districtName = districtUnit.Name;
            }

            var vm = new HomeViewModel
            {
                UserFullName = user.FullName,
                WardName = wardName,
                DistrictName = districtName,
                MyPendingCount = myPendingCount,
                LatestPolicies = latestPolicies,
                FeaturedIssues = featuredIssues,
                MyRecentIssues = myRecentIssues
            };

            ViewData["Title"] = "Trang chủ";
            return View(vm);
        }

        private static string GetCategoryName(IssueCategory category) => category switch
        {
            IssueCategory.Traffic => "Giao thông",
            IssueCategory.Environment => "Môi trường",
            IssueCategory.Security => "An ninh trật tự",
            IssueCategory.Infrastructure => "Hạ tầng",
            IssueCategory.Administration => "Hành chính",
            _ => "Khác"
        };

        private static string GetCategoryClass(IssueCategory category) => category switch
        {
            IssueCategory.Traffic => "cat-traffic",
            IssueCategory.Environment => "cat-environment",
            IssueCategory.Security => "cat-security",
            IssueCategory.Infrastructure => "cat-infra",
            _ => "cat-other"
        };

        private static string GetStatusName(IssueStatus status) => status switch
        {
            IssueStatus.Pending => "Chờ tiếp nhận",
            IssueStatus.Assigned => "Đã phân công",
            IssueStatus.Processing => "Đang xử lý",
            IssueStatus.Resolved => "Đã giải quyết",
            IssueStatus.Rejected => "Từ chối",
            IssueStatus.Closed => "Đóng tự động",
            _ => "Không xác định"
        };

        private static string GetStatusClass(IssueStatus status) => status switch
        {
            IssueStatus.Pending => "pending",
            IssueStatus.Assigned => "assigned",
            IssueStatus.Processing => "processing",
            IssueStatus.Resolved => "resolved",
            IssueStatus.Rejected => "rejected",
            _ => "closed"
        };

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            ViewData["PageHeader"] = "Chính Sách & Điều Khoản";
            return View();
        }

        [AllowAnonymous]
        public IActionResult About()
        {
            ViewData["Title"] = "Giới thiệu";
            return View();
        }

        [AllowAnonymous]
        public IActionResult Guide()
        {
            ViewData["Title"] = "Hướng dẫn sử dụng";
            return View();
        }
    }
}

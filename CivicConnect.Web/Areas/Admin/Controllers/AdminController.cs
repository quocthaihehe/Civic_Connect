using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("AdminOld/[action]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIssueService _issueService;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager, IIssueService issueService)
        {
            _context = context;
            _userManager = userManager;
            _issueService = issueService;
        }

        [HttpGet]
        [Route("/AdminOld")]
        [Route("/AdminOld/Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            ViewData["PageHeader"] = "Trang tổng quan";
            
            ViewData["TotalReports"] = await _context.Issues.CountAsync();
            ViewData["InProgress"] = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Processing || i.Status == IssueStatus.Assigned);
            ViewData["Resolved"] = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved);
            ViewData["UrgentCases"] = await _context.Issues.CountAsync(i => i.Priority == IssuePriority.Critical && i.Status != IssueStatus.Resolved && i.Status != IssueStatus.Closed);
            
            var issues = await _context.Issues
                .Include(i => i.Author)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(issues);
        }

        [HttpGet]
        [Route("/AdminOld/Issues")]
        public async Task<IActionResult> Issues(IssueCategory? category, IssueStatus? status, IssuePriority? priority, string? search)
        {
            ViewData["PageHeader"] = "Quản Lý Phản Ánh";
            var query = _context.Issues.Include(i => i.Author).AsQueryable();

            if (category.HasValue)
                query = query.Where(i => i.Category == category.Value);
            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);
            if (priority.HasValue)
                query = query.Where(i => i.Priority == priority.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(i => i.Title.Contains(search) || i.Description.Contains(search) || i.Address.Contains(search));

            var issues = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return View(issues);
        }

        [HttpGet]
        [Route("/AdminOld/Issues/Details/{id}")]
        public async Task<IActionResult> IssueDetails(int id)
        {
            ViewData["PageHeader"] = "Chi Tiết Phản Ánh";
            var issue = await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Comments).ThenInclude(c => c.Author)
                .Include(i => i.StatusHistory).ThenInclude(h => h.ChangedBy)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null)
                return NotFound();

            var officials = await _userManager.Users.ToListAsync();
            ViewData["Officials"] = officials;

            return View(issue);
        }

        [HttpPost]
        [Route("/AdminOld/Issues/UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, IssueStatus status, string? note)
        {
            var officialId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(officialId)) return Unauthorized();

            bool success = false;
            if (status == IssueStatus.Assigned)
                success = await _issueService.AcceptIssueAsync(id, officialId);
            else if (status == IssueStatus.Processing)
                success = await _issueService.ProcessIssueAsync(id, officialId);
            else if (status == IssueStatus.Resolved)
                success = await _issueService.ResolveIssueAsync(id, officialId, note, null);
            else if (status == IssueStatus.Rejected)
                success = await _issueService.RejectIssueAsync(id, officialId, note ?? "Từ chối xử lý");

            if (success)
                TempData["SuccessMessage"] = "Cập nhật trạng thái phản ánh thành công!";
            else
                TempData["ErrorMessage"] = "Cập nhật trạng thái thất bại.";

            return RedirectToAction(nameof(IssueDetails), new { id = id });
        }

        [HttpPost]
        [Route("/AdminOld/Issues/Assign")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, string assignedToUserId, string? note)
        {
            var officialId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(officialId)) return Unauthorized();

            var success = await _issueService.AssignIssueAsync(id, officialId, assignedToUserId, note);
            if (success)
                TempData["SuccessMessage"] = "Đã phân công phản ánh thành công!";
            else
                TempData["ErrorMessage"] = "Phân công phản ánh thất bại.";

            return RedirectToAction(nameof(IssueDetails), new { id = id });
        }

        [HttpPost]
        [Route("/AdminOld/Issues/AddComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string content)
        {
            var authorId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(authorId)) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(content))
            {
                await _issueService.AddCommentAsync(id, authorId, content, null, true);
                TempData["SuccessMessage"] = "Đã đăng phản hồi chính thức thành công!";
            }

            return RedirectToAction(nameof(IssueDetails), new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            ViewData["PageHeader"] = "Quản Lý Tài Khoản";
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive; // Thay đổi trạng thái khóa
                await _userManager.UpdateAsync(user);
                
                // Cưỡng chế đăng xuất nếu khóa
                if (!user.IsActive)
                {
                    await _userManager.UpdateSecurityStampAsync(user);
                }

                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái hoạt động của tài khoản {user.Email}!";
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> Units()
        {
            var units = await _context.GovernmentUnits
                .Include(u => u.ParentUnit)
                .ToListAsync();
            ViewData["PageHeader"] = "Danh Sách Cơ Quan";
            return View(units);
        }

        [HttpGet]
        [Route("/AdminOld/Policies")]
        public async Task<IActionResult> Policies()
        {
            ViewData["PageHeader"] = "Quản Lý Chính Sách";
            var policies = await _context.Policies
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();
            return View(policies);
        }

        [HttpPost]
        [Route("/AdminOld/Policies/Create")]
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

            await _context.Policies.AddAsync(policy);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo mới chính sách/thông báo thành công!";
            return RedirectToAction(nameof(Policies));
        }

        [HttpGet]
        [Route("/AdminOld/Analytics")]
        public async Task<IActionResult> Analytics()
        {
            ViewData["PageHeader"] = "Thống Kê & Báo Cáo";

            ViewData["TotalCount"] = await _context.Issues.CountAsync();
            ViewData["PendingCount"] = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Pending);
            ViewData["ProcessingCount"] = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Processing || i.Status == IssueStatus.Assigned);
            ViewData["ResolvedCount"] = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved);
            ViewData["RejectedCount"] = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Rejected);

            var issueCategories = await _context.Issues
                .Select(i => new { i.Category })
                .ToListAsync();
            var byCategory = issueCategories
                .GroupBy(i => i.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewData["ByCategory"] = byCategory;

            return View();
        }

        [HttpGet]
        [Route("/AdminOld/Settings")]
        public async Task<IActionResult> Settings()
        {
            ViewData["PageHeader"] = "Cấu Hình Hệ Thống";

            ViewData["UsersCount"] = await _context.Users.CountAsync();
            ViewData["UnitsCount"] = await _context.GovernmentUnits.CountAsync();
            ViewData["IssuesCount"] = await _context.Issues.CountAsync();
            ViewData["CommentsCount"] = await _context.Comments.CountAsync();

            return View();
        }
    }
}

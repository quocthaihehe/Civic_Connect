using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            ViewData["PageHeader"] = "Quản Trị Hệ Thống";
            
            ViewData["UserCount"] = await _context.Users.CountAsync();
            ViewData["IssueCount"] = await _context.Issues.CountAsync();
            ViewData["UnitCount"] = await _context.GovernmentUnits.CountAsync();
            
            var issues = await _context.Issues
                .Include(i => i.Author)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(issues);
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
        public async Task<IActionResult> UnitStats()
        {
            ViewData["PageHeader"] = "Thống Kê Uy Tín Cơ Quan";

            var unitStats = await _context.GovernmentUnits
                .Select(gu => new
                {
                    gu.Id,
                    gu.Name,
                    gu.Type,
                    TotalAssigned = _context.Issues.Count(i => i.AssignedUnitId == gu.Id),
                    TotalResolved = _context.Issues.Count(i => i.AssignedUnitId == gu.Id && i.Status == CivicConnect.Web.Models.Enums.IssueStatus.Resolved),
                    RatedCount = _context.Issues.Count(i => i.AssignedUnitId == gu.Id && i.SatisfactionRating != null),
                    AvgRating = _context.Issues
                        .Where(i => i.AssignedUnitId == gu.Id && i.SatisfactionRating != null)
                        .Select(i => (double?)i.SatisfactionRating)
                        .Average()
                })
                .ToListAsync();

            ViewData["UnitStats"] = unitStats;
            return View("UnitStats", unitStats);
        }

        [HttpGet]
        public async Task<IActionResult> Issues()
        {
            var issues = await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.AssignedUnit)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            
            ViewData["GovernmentUnits"] = await _context.GovernmentUnits.ToListAsync();
            ViewData["PageHeader"] = "Quản Lý Phản Ánh";
            return View(issues);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyIssue(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null)
            {
                issue.IsVerified = true;
                issue.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã duyệt và xác thực phản ánh #{id}!";
            }
            return RedirectToAction(nameof(Issues));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectIssue(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null && issue.Status != Models.Enums.IssueStatus.Resolved && issue.Status != Models.Enums.IssueStatus.Closed)
            {
                var oldStatus = issue.Status;
                issue.Status = Models.Enums.IssueStatus.Rejected;
                issue.UpdatedAt = DateTime.UtcNow;
                
                var history = new IssueStatusHistory
                {
                    IssueId = issue.Id,
                    FromStatus = oldStatus,
                    ToStatus = Models.Enums.IssueStatus.Rejected,
                    ChangedById = _userManager.GetUserId(User) ?? "",
                    Note = "Quản trị viên từ chối phản ánh không hợp lệ.",
                    ChangedAt = DateTime.UtcNow
                };
                await _context.IssueStatusHistories.AddAsync(history);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Đã từ chối phản ánh #{id}!";
            }
            return RedirectToAction(nameof(Issues));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignIssue(int id, string assignedUnitId)
        {
            var issue = await _context.Issues.FindAsync(id);
            var unit = await _context.GovernmentUnits.FindAsync(assignedUnitId);
            if (issue != null && unit != null)
            {
                var oldStatus = issue.Status;
                issue.AssignedUnitId = assignedUnitId;
                issue.Status = Models.Enums.IssueStatus.Assigned;
                issue.AssignedAt = DateTime.UtcNow;
                issue.UpdatedAt = DateTime.UtcNow;
                
                var history = new IssueStatusHistory
                {
                    IssueId = issue.Id,
                    FromStatus = oldStatus,
                    ToStatus = Models.Enums.IssueStatus.Assigned,
                    ChangedById = _userManager.GetUserId(User) ?? "",
                    Note = $"Quản trị viên điều phối sang đơn vị: {unit.Name}.",
                    ChangedAt = DateTime.UtcNow
                };
                await _context.IssueStatusHistories.AddAsync(history);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Đã điều phối phản ánh #{id} sang {unit.Name}!";
            }
            return RedirectToAction(nameof(Issues));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIssue(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa phản ánh #{id} khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Issues));
        }

        [HttpGet]
        public async Task<IActionResult> Donations()
        {
            var campaigns = await _context.DonationCategories
                .OrderByDescending(dc => dc.CreatedAt)
                .ToListAsync();
            
            var donations = await _context.Donations
                .Include(d => d.DonationCategory)
                .Include(d => d.User)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            ViewData["Donations"] = donations;
            ViewData["PageHeader"] = "Quản Lý Quỹ Quyên Góp";
            return View(campaigns);
        }

        [HttpGet]
        public IActionResult CreateCampaign()
        {
            ViewData["PageHeader"] = "Tạo Chiến Dịch Quyên Góp";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCampaign(DonationCategory model)
        {
            if (ModelState.IsValid)
            {
                model.CurrentAmount = 0;
                model.IsActive = true;
                model.CreatedAt = DateTime.UtcNow;
                
                await _context.DonationCategories.AddAsync(model);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Tạo chiến dịch '{model.Name}' thành công!";
                return RedirectToAction(nameof(Donations));
            }
            ViewData["PageHeader"] = "Tạo Chiến Dịch Quyên Góp";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCampaign(int id)
        {
            var campaign = await _context.DonationCategories.FindAsync(id);
            if (campaign != null)
            {
                campaign.IsActive = !campaign.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thay đổi trạng thái hoạt động của chiến dịch '{campaign.Name}'!";
            }
            return RedirectToAction(nameof(Donations));
        }

        [HttpGet]
        public IActionResult CreatePolicy()
        {
            ViewData["PageHeader"] = "Đăng Tin tức / Chính sách";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePolicy(Policy model)
        {
            if (ModelState.IsValid)
            {
                model.IsActive = true;
                model.PublishedDate = DateTime.UtcNow;
                model.EffectiveDate = DateTime.UtcNow;
                if (string.IsNullOrEmpty(model.TagClass))
                {
                    model.TagClass = model.Tag switch
                    {
                        "Nghị định" => "tag-law",
                        "Thông tư" => "tag-policy",
                        "Thông báo" => "tag-notice",
                        _ => "tag-news"
                    };
                }
                
                await _context.Policies.AddAsync(model);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Đăng tin bài '{model.Title}' thành công!";
                return RedirectToAction("Index", "Policy", new { area = "" });
            }
            ViewData["PageHeader"] = "Đăng Tin tức / Chính sách";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id, int issueId)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa bình luận thành công!";
            }
            return RedirectToAction("Details", "Issues", new { area = "", id = issueId });
        }
    }
}

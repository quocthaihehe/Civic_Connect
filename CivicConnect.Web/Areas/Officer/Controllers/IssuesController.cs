using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Officer.Controllers
{
    [Area("Officer")]
    [Authorize(Roles = "OfficialDistrict, OfficialWard")]
    public class IssuesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IssuesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(IssueStatus? statusFilter, IssueCategory? categoryFilter)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            bool isDistrictOfficial = roles.Contains("OfficialDistrict");

            IQueryable<Issue> query = _context.Issues
                .Include(i => i.Author);

            // Territory isolation
            if (isDistrictOfficial)
            {
                query = query.Where(i => i.DistrictCode == user.DistrictCode);
            }
            else
            {
                query = query.Where(i => i.WardCode == user.WardCode);
            }

            // Filters
            if (statusFilter.HasValue)
            {
                query = query.Where(i => i.Status == statusFilter.Value);
            }
            
            if (categoryFilter.HasValue)
            {
                query = query.Where(i => i.Category == categoryFilter.Value);
            }

            var issues = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            
            // Generate list of categories for filter dropdown
            ViewBag.Categories = await _context.Issues.Select(i => i.Category).Distinct().ToListAsync();
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.CurrentCategory = categoryFilter;

            return View(issues);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            bool isDistrictOfficial = roles.Contains("OfficialDistrict");

            var issue = await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Comments).ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null) return NotFound();

            // Check authorization
            if (isDistrictOfficial && issue.DistrictCode != user.DistrictCode) return Forbid();
            if (!isDistrictOfficial && issue.WardCode != user.WardCode) return Forbid();

            return View(issue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, IssueStatus newStatus, string processingNote)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            bool isDistrictOfficial = roles.Contains("OfficialDistrict");

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            // Check authorization
            if (isDistrictOfficial && issue.DistrictCode != user.DistrictCode) return Forbid();
            if (!isDistrictOfficial && issue.WardCode != user.WardCode) return Forbid();

            // Cannot go backwards (Forward-only Kanban logic applied here as well)
            if (newStatus < issue.Status && newStatus != IssueStatus.Rejected)
            {
                TempData["ErrorMessage"] = "Không thể chuyển trạng thái lùi.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            issue.Status = newStatus;
            
            if (newStatus == IssueStatus.Assigned)
            {
                issue.AssignedToUserId = user.Id;
            }

            // Log activity
            var auditLog = new AuditLog
            {
                UserId = user.Id,
                Action = "UpdateIssueStatus",
                Details = $"Issue #{issue.Id} -> Trạng thái: {newStatus}. Ghi chú: {processingNote}",
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}

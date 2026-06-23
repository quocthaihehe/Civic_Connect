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
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var roles = await _userManager.GetRolesAsync(user);
            bool isDistrictOfficial = roles.Contains("OfficialDistrict");
            
            // Build query based on territory
            IQueryable<Issue> query = _context.Issues;
            if (isDistrictOfficial)
            {
                query = query.Where(i => i.DistrictCode == user.DistrictCode);
            }
            else
            {
                query = query.Where(i => i.WardCode == user.WardCode);
            }

            // Calculate KPIs
            var totalIssues = await query.CountAsync();
            var pendingIssues = await query.CountAsync(i => i.Status == IssueStatus.Pending);
            var processingIssues = await query.CountAsync(i => i.Status == IssueStatus.Processing || i.Status == IssueStatus.Assigned);
            var resolvedIssues = await query.CountAsync(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed);

            var thisMonth = DateTime.UtcNow.Month;
            var issuesThisMonth = await query.CountAsync(i => i.CreatedAt.Month == thisMonth);

            // Chart data: Issues by Category
            var issuesByCategory = await query
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewBag.TotalIssues = totalIssues;
            ViewBag.PendingIssues = pendingIssues;
            ViewBag.ProcessingIssues = processingIssues;
            ViewBag.ResolvedIssues = resolvedIssues;
            ViewBag.IssuesThisMonth = issuesThisMonth;
            
            ViewBag.Categories = issuesByCategory.Select(c => c.Category).ToList();
            ViewBag.CategoryCounts = issuesByCategory.Select(c => c.Count).ToList();

            // Recent pending issues to display on dashboard
            var recentPending = await query
                .Where(i => i.Status == IssueStatus.Pending)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentPending = recentPending;
            ViewBag.TerritoryName = isDistrictOfficial ? "Quận/Huyện" : "Phường/Xã";

            return View();
        }
    }
}

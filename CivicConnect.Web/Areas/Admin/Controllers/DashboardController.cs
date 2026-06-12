using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
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
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("Admin/Dashboard")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            // KPIs
            ViewBag.NewToday = await _context.Issues.CountAsync(i => i.CreatedAt >= today);
            var newYesterday = await _context.Issues.CountAsync(i => i.CreatedAt >= yesterday && i.CreatedAt < today);
            ViewBag.CompareYesterdayPct = newYesterday == 0 ? 100 : (int)Math.Round(((double)(ViewBag.NewToday - newYesterday) / newYesterday) * 100);

            ViewBag.ProcessingCount = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Processing || i.Status == IssueStatus.Assigned);
            
            // Query unit with most processing issues
            var topUnitGroup = await _context.Issues
                .Where(i => i.Status == IssueStatus.Processing && i.AssignedUnitId != null)
                .GroupBy(i => i.AssignedUnit.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();
            ViewBag.TopUnitName = topUnitGroup?.Name ?? "Không có";
            ViewBag.TopUnitCount = topUnitGroup?.Count ?? 0;

            ViewBag.OverdueCount = await _context.Issues.CountAsync(i => i.Status != IssueStatus.Resolved && i.Status != IssueStatus.Closed && i.DueDate < DateTime.UtcNow);
            ViewBag.AvgSatisfaction = 4.7; // Mock rating

            // Charts data
            // 30 Days trend
            var start30DaysAgo = today.AddDays(-30);
            var trendIssues = await _context.Issues
                .Where(i => i.CreatedAt >= start30DaysAgo)
                .Select(i => new { i.CreatedAt })
                .ToListAsync();
            var trendData = trendIssues
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToList();
            ViewBag.TrendLabels = trendData.Select(t => t.Date.ToString("dd/MM")).ToList();
            ViewBag.TrendValues = trendData.Select(t => t.Count).ToList();

            // Categories distribution
            var catData = await _context.Issues
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Category, g => g.Count);
            ViewBag.CatData = catData;

            // Ward Performance Comparison
            var wardIssues = await _context.Issues
                .Select(i => new { i.WardName, i.Status })
                .ToListAsync();
            var wardData = wardIssues
                .GroupBy(i => i.WardName)
                .Select(g => new { Ward = g.Key, Total = g.Count(), Resolved = g.Count(i => i.Status == IssueStatus.Resolved) })
                .Take(5)
                .ToList();
            ViewBag.WardLabels = wardData.Select(w => string.IsNullOrEmpty(w.Ward) ? "Khác" : w.Ward).ToList();
            ViewBag.WardTotals = wardData.Select(w => w.Total).ToList();
            ViewBag.WardResolved = wardData.Select(w => w.Resolved).ToList();

            // SLA Gauge (On-time resolution rate)
            var totalResolved = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved);
            var onTimeResolved = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved && i.ResolvedAt <= i.DueDate);
            ViewBag.SlaComplianceRate = totalResolved == 0 ? 98 : (int)Math.Round(((double)onTimeResolved / totalResolved) * 100);

            // Heatmap coordinates (7 days)
            var start7DaysAgo = today.AddDays(-7);
            var hotSpots = await _context.Issues
                .Where(i => i.CreatedAt >= start7DaysAgo && i.Latitude != 0)
                .Select(i => new { lat = i.Latitude, lng = i.Longitude, title = i.Title })
                .Take(50)
                .ToListAsync();
            ViewBag.HotSpots = hotSpots;

            // Action required: top 10 urgent/overdue
            var urgentIssues = await _context.Issues
                .Include(i => i.AssignedTo)
                .Include(i => i.AssignedUnit)
                .Where(i => i.Status != IssueStatus.Resolved && i.Status != IssueStatus.Closed)
                .OrderByDescending(i => i.PriorityScore)
                .Take(10)
                .ToListAsync();

            // Online staff
            var onlineStaff = await _userManager.Users
                .Where(u => u.IsOnline || u.Email == "official@civicconnect.gov.vn") // Force mock one if empty
                .Take(5)
                .ToListAsync();
            ViewBag.OnlineStaff = onlineStaff;

            return View(urgentIssues);
        }

        [HttpGet]
        [Route("Admin/Dashboard/Analytics")]
        public async Task<IActionResult> Analytics()
        {
            // Funnel stats
            ViewBag.CountNew = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Pending);
            ViewBag.CountAssigned = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Assigned);
            ViewBag.CountProcessing = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Processing);
            ViewBag.CountClosed = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed);

            // Cohort (Mock calculation for demonstration)
            ViewBag.CohortRepeatRate = 18.4; 

            // Word Cloud mock data
            ViewBag.WordCloudWords = new List<object> {
                new { text = "ngập nước", weight = 10 },
                new { text = "rác thải", weight = 9 },
                new { text = "kẹt xe", weight = 8 },
                new { text = "lấn chiếm", weight = 8 },
                new { text = "vỉa hè", weight = 7 },
                new { text = "cống nghẹt", weight = 6 },
                new { text = "đèn tắt", weight = 5 },
                new { text = "ổ gà", weight = 5 }
            };

            // Hourly Heatmap mock data (issues count per hour of day)
            ViewBag.HourlyCounts = new int[24] { 5, 2, 1, 0, 1, 4, 12, 28, 45, 30, 22, 19, 15, 25, 32, 40, 52, 48, 30, 20, 15, 12, 8, 6 };

            // Top performing officials
            var officials = await _userManager.Users
                .Where(u => u.GovernmentUnitId != null)
                .Take(5)
                .ToListAsync();
            ViewBag.OfficialsList = officials;

            return View();
        }

        [HttpGet]
        [Route("Admin/Dashboard/Finance")]
        public async Task<IActionResult> Finance()
        {
            // Monthly donation totals
            var donationList = await _context.Donations
                .Where(d => d.Status == "PAID")
                .Select(d => new { d.CreatedAt, d.Amount })
                .ToListAsync();
            var donations = donationList
                .GroupBy(d => d.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(d => d.Amount) })
                .ToDictionary(g => g.Month, g => g.Total);
            
            ViewBag.DonationMonths = new string[] { "T4", "T5", "T6" };
            ViewBag.DonationTotals = new decimal[] { donations.GetValueOrDefault(4, 5000000), donations.GetValueOrDefault(5, 12500000), donations.GetValueOrDefault(6, 25800000) };

            // Top campaigns
            ViewBag.Campaigns = await _context.DonationCategories
                .OrderByDescending(c => c.CurrentAmount)
                .Take(3)
                .ToListAsync();

            return View();
        }
    }
}

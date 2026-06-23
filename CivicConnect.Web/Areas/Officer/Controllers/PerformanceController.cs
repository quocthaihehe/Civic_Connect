using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Officer.Controllers
{
    [Area("Officer")]
    [Authorize(Roles = "OfficialDistrict, OfficialWard")]
    public class PerformanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PerformanceController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            bool isDistrictOfficial = roles.Contains("OfficialDistrict");

            // KPI của cá nhân
            var totalAssigned = await _context.Issues.CountAsync(i => i.AssignedToUserId == user.Id);
            var totalResolved = await _context.Issues.CountAsync(i => i.AssignedToUserId == user.Id && (i.Status == IssueStatus.Resolved || i.Status == IssueStatus.Closed));
            var resolutionRate = totalAssigned > 0 ? (totalResolved * 100.0 / totalAssigned) : 0;

            ViewBag.TotalAssigned = totalAssigned;
            ViewBag.TotalResolved = totalResolved;
            ViewBag.ResolutionRate = resolutionRate;
            ViewBag.PerformanceScore = user.TrustScore; // Giả định TrustScore được dùng chung làm KPI score cho cán bộ

            // Xếp hạng (So với các cán bộ khác cùng cấp)
            IQueryable<ApplicationUser> peersQuery = _userManager.Users;
            if (isDistrictOfficial)
            {
                peersQuery = peersQuery.Where(u => u.DistrictCode == user.DistrictCode && u.WardCode == null);
            }
            else
            {
                peersQuery = peersQuery.Where(u => u.WardCode == user.WardCode);
            }

            var peers = await peersQuery
                .Select(u => new { u.Id, u.FullName, u.UserName, u.TrustScore })
                .OrderByDescending(u => u.TrustScore)
                .ToListAsync();

            ViewBag.Peers = peers;
            ViewBag.MyRank = peers.FindIndex(p => p.Id == user.Id) + 1;

            return View();
        }
    }
}

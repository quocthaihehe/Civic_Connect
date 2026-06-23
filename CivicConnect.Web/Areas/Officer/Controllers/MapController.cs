using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
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
    public class MapController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MapController(AppDbContext context, UserManager<ApplicationUser> userManager)
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

            // Chỉ lấy các sự cố có toạ độ hợp lệ
            var issues = await query
                .Where(i => i.Latitude != 0 && i.Longitude != 0)
                .Select(i => new {
                    i.Id,
                    i.Title,
                    i.Category,
                    i.Status,
                    i.Latitude,
                    i.Longitude,
                    i.Address
                })
                .ToListAsync();

            ViewBag.IssuesData = System.Text.Json.JsonSerializer.Serialize(issues);
            return View();
        }
    }
}

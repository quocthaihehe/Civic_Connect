using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
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
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnnouncementsController(AppDbContext context, UserManager<ApplicationUser> userManager)
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

            // Lấy danh sách thông báo đã đăng bởi cán bộ này
            var announcements = await _context.Policies
                .Where(p => p.IssuingUnit == (isDistrictOfficial ? user.DistrictCode : user.WardCode))
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();

            return View(announcements);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string content, string type)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            bool isDistrictOfficial = roles.Contains("OfficialDistrict");
            var unitCode = isDistrictOfficial ? user.DistrictCode : user.WardCode;

            var policy = new Policy
            {
                Title = title,
                Excerpt = content.Length > 100 ? content.Substring(0, 100) + "..." : content,
                Content = content,
                Tag = type, // "Cảnh báo khẩn", "Thông báo"
                TagClass = type == "Cảnh báo khẩn" ? "tag-danger" : "tag-notice",
                IssuingUnit = unitCode,
                PublishedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Policies.Add(policy);
            
            // TODO: Bắn notification (Bulk) cho user ở khu vực này. Tạm thời bỏ qua phần Bulk SMS/Notif phức tạp
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đăng thông báo thành công.";

            return RedirectToAction(nameof(Index));
        }
    }
}

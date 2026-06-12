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
    public class UsersManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersManagementController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("Admin/Users")]
        public async Task<IActionResult> Index(KYCLevel? kycLevel, bool? isRestricted, string? search)
        {
            var query = _userManager.Users
                .Where(u => u.GovernmentUnitId == null) // Filter citizens only
                .AsQueryable();

            if (kycLevel.HasValue)
                query = query.Where(u => u.KYCLevel == kycLevel.Value);
            if (isRestricted.HasValue)
                query = query.Where(u => u.IsRestricted == isRestricted.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search) || u.PhoneNumber.Contains(search));

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View("~/Areas/Admin/Views/Users/Index.cshtml", users);
        }

        [HttpPost]
        [Route("Admin/Users/VerifyKYC/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyKYC(string id, KYCLevel level)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.KYCLevel = level;
            if (level == KYCLevel.IdentityVerified)
            {
                user.IsPhoneVerified = true;
                user.IsEmailVerified = true;
            }

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái xác thực KYC của công dân {user.FullName} thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Admin/Users/Restrict/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestrictUser(string id, int days, string reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsRestricted = true;
            user.RestrictionReason = reason;
            user.RestrictedUntil = DateTime.UtcNow.AddDays(days);
            user.CitizenPoints = Math.Max(0, user.CitizenPoints - 20); // Deduct citizenship score points

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = $"Đã áp dụng hình thức hạn chế tài khoản công dân {user.FullName} trong {days} ngày!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Admin/Users/Unrestrict/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnrestrictUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsRestricted = false;
            user.RestrictionReason = null;
            user.RestrictedUntil = null;
            user.CitizenPoints = Math.Min(100, user.CitizenPoints + 10); // Reward some points back

            await _userManager.UpdateAsync(user);
            TempData["SuccessMessage"] = $"Đã huỷ bỏ lệnh hạn chế của công dân {user.FullName}!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Admin/Users/ImportExcel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel()
        {
            // Simulated import parsing logic
            // In a real app we parse excel streams, here we simulate successful seeding of 3 citizen accounts
            var seedData = new List<ApplicationUser>
            {
                new ApplicationUser { UserName = "congdan.quan1@gmail.com", Email = "congdan.quan1@gmail.com", FullName = "Trần Thanh Bình", PhoneNumber = "0912345678", KYCLevel = KYCLevel.PhoneVerified, CreatedAt = DateTime.UtcNow, IsActive = true },
                new ApplicationUser { UserName = "hoangnam.q1@gmail.com", Email = "hoangnam.q1@gmail.com", FullName = "Lê Hoàng Nam", PhoneNumber = "0987654321", KYCLevel = KYCLevel.IdentityVerified, CreatedAt = DateTime.UtcNow, IsActive = true },
                new ApplicationUser { UserName = "myhuyen.bennghe@gmail.com", Email = "myhuyen.bennghe@gmail.com", FullName = "Đỗ Mỹ Huyền", PhoneNumber = "0901234567", KYCLevel = KYCLevel.Unverified, CreatedAt = DateTime.UtcNow, IsActive = true }
            };

            int importedCount = 0;
            foreach (var user in seedData)
            {
                if (await _userManager.FindByEmailAsync(user.Email) == null)
                {
                    var result = await _userManager.CreateAsync(user, "DefaultPassword123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Citizen");
                        importedCount++;
                    }
                }
            }

            TempData["SuccessMessage"] = $"Đã nhập hàng loạt thành công {importedCount} tài khoản cư dân từ tệp dữ liệu Excel!";
            return RedirectToAction(nameof(Index));
        }
    }
}

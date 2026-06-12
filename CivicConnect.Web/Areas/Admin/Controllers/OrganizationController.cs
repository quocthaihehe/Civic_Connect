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
    public class OrganizationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrganizationController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- B.3.2 Quản lý Đơn vị (Units) ---
        [HttpGet]
        [Route("Admin/Units")]
        public async Task<IActionResult> Index()
        {
            var units = await _context.GovernmentUnits
                .Include(u => u.ParentUnit)
                .Include(u => u.ChildUnits)
                .ToListAsync();
            return View("~/Areas/Admin/Views/Units/Index.cshtml", units);
        }

        [HttpPost]
        [Route("Admin/Units/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUnit(string id, string name, GovernmentUnitType type, string? parentUnitId, string provinceCode, string? districtCode, string? wardCode, string? address, string? phone, string? email)
        {
            if (await _context.GovernmentUnits.AnyAsync(u => u.Id == id))
            {
                TempData["ErrorMessage"] = "Mã cơ quan đã tồn tại trên hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            var unit = new GovernmentUnit
            {
                Id = id,
                Name = name,
                Type = type,
                ParentUnitId = string.IsNullOrEmpty(parentUnitId) ? null : parentUnitId,
                ProvinceCode = provinceCode,
                DistrictCode = districtCode,
                WardCode = wardCode,
                Address = address,
                Phone = phone,
                Email = email,
                IsActive = true
            };

            _context.GovernmentUnits.Add(unit);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã tạo cơ quan liên kết mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("Admin/Units/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnit(string id)
        {
            var unit = await _context.GovernmentUnits.FindAsync(id);
            if (unit != null)
            {
                _context.GovernmentUnits.Remove(unit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xoá cơ quan khỏi hệ thống!";
            }
            return RedirectToAction(nameof(Index));
        }

        // --- B.3.3 Quản lý Tài khoản Cán bộ (Officials) ---
        [HttpGet]
        [Route("Admin/Officials")]
        public async Task<IActionResult> Officials()
        {
            var officials = await _userManager.Users
                .Include(u => u.GovernmentUnit)
                .Where(u => u.GovernmentUnitId != null)
                .ToListAsync();

            ViewBag.Units = await _context.GovernmentUnits.ToListAsync();
            return View("~/Areas/Admin/Views/Officials/Index.cshtml", officials);
        }

        [HttpPost]
        [Route("Admin/Officials/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOfficial(string fullName, string email, string phoneNumber, string governmentUnitId, string password)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                GovernmentUnitId = governmentUnitId,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Assign a default Official role based on unit type
                var unit = await _context.GovernmentUnits.FindAsync(governmentUnitId);
                string role = "DepartmentStaff";
                if (unit != null)
                {
                    role = unit.Type switch
                    {
                        GovernmentUnitType.Province => "OfficialProvince",
                        GovernmentUnitType.District => "OfficialDistrict",
                        GovernmentUnitType.Ward => "OfficialWard",
                        _ => "DepartmentStaff"
                    };
                }
                await _userManager.AddToRoleAsync(user, role);

                TempData["SuccessMessage"] = "Đã tạo mới tài khoản cán bộ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi tạo cán bộ: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Officials));
        }

        [HttpPost]
        [Route("Admin/Officials/Lock/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockOfficial(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái hoạt động cán bộ {user.FullName}!";
            }
            return RedirectToAction(nameof(Officials));
        }

        // --- B.3.4 Ca trực & Lịch làm việc (Shifts) ---
        [HttpGet]
        [Route("Admin/Officials/Shifts")]
        public async Task<IActionResult> Shifts()
        {
            var shifts = await _context.Shifts
                .Include(s => s.User)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            ViewBag.Officials = await _userManager.Users.Where(u => u.GovernmentUnitId != null).ToListAsync();
            return View("~/Areas/Admin/Views/Officials/Shifts.cshtml", shifts);
        }

        [HttpPost]
        [Route("Admin/Officials/CreateShift")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateShift(string userId, DateTime startTime, DateTime endTime, string? notes)
        {
            var shift = new Shift
            {
                UserId = userId,
                StartTime = startTime,
                EndTime = endTime,
                Notes = notes,
                IsActive = true
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lên lịch ca trực cho cán bộ thành công!";
            return RedirectToAction(nameof(Shifts));
        }
    }
}

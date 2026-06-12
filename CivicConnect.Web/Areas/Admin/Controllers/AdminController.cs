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
    }
}

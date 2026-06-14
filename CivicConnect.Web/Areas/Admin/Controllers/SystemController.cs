using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
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
    public class SystemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SystemController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Route("Admin/System/Settings")]
        public async Task<IActionResult> Settings()
        {
            var orgName = await _context.SystemSettings.FindAsync("OrganizationName");
            var maintMode = await _context.SystemSettings.FindAsync("MaintenanceMode");

            ViewBag.OrgName = orgName?.SettingValue ?? "CivicConnect";
            ViewBag.MaintenanceMode = maintMode?.SettingValue ?? "False";

            return View();
        }

        [HttpPost]
        [Route("Admin/System/SaveSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(string orgName, string maintenanceMode)
        {
            var orgSetting = await _context.SystemSettings.FindAsync("OrganizationName");
            if (orgSetting != null)
            {
                orgSetting.SettingValue = orgName;
                orgSetting.UpdatedAt = DateTime.UtcNow;
            }

            var maintSetting = await _context.SystemSettings.FindAsync("MaintenanceMode");
            if (maintSetting != null)
            {
                maintSetting.SettingValue = maintenanceMode;
                maintSetting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Log activity
            var adminId = _userManager.GetUserId(User) ?? "System";
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminId,
                Action = "UPDATE_SETTINGS",
                Details = $"Cập nhật tên tổ chức: {orgName}, Bảo trì: {maintenanceMode}",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật cấu hình hệ thống thành công!";
            return RedirectToAction(nameof(Settings));
        }

        [HttpGet]
        [Route("Admin/System/Logs")]
        public async Task<IActionResult> Logs(string? search)
        {
            var query = _context.AuditLogs
                .Include(l => l.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.Action.Contains(search) || l.Details.Contains(search));
            }

            var logs = await query.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync();
            return View(logs);
        }

        [HttpGet]
        [Route("Admin/System/Health")]
        public IActionResult Health()
        {
            // Gather CPU and RAM mock values dynamically
            var random = new Random();
            ViewBag.CpuUsage = random.Next(15, 65);
            ViewBag.RamUsage = random.Next(40, 85);
            ViewBag.DbConnection = "Ổn định (Khỏe mạnh)";
            ViewBag.QueueLength = random.Next(0, 5);

            return View();
        }

        [HttpPost]
        [Route("Admin/System/TriggerBackup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TriggerBackup()
        {
            // Simulate SQL database backup file generation
            var csvContent = "Backup SQL Database simulated at " + DateTime.Now.ToString();
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

            var adminId = _userManager.GetUserId(User) ?? "System";
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = adminId,
                Action = "DATABASE_BACKUP",
                Details = "Sao lưu CSDL thủ công",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return File(bytes, "application/octet-stream", $"CivicConnect_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        }
    }
}

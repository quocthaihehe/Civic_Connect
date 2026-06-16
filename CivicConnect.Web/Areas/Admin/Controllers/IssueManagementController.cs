using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class IssueManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIssueService _issueService;

        public IssueManagementController(AppDbContext context, UserManager<ApplicationUser> userManager, IIssueService issueService)
        {
            _context = context;
            _userManager = userManager;
            _issueService = issueService;
        }

        [HttpGet]
        [Route("Admin/Issues")]
        public async Task<IActionResult> Index(IssueCategory? category, IssueStatus? status, IssuePriority? priority, string? search)
        {
            var query = _context.Issues
                .Include(i => i.Author)
                .Include(i => i.AssignedTo)
                .Include(i => i.AssignedUnit)
                .AsQueryable();

            if (category.HasValue)
                query = query.Where(i => i.Category == category.Value);
            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);
            if (priority.HasValue)
                query = query.Where(i => i.Priority == priority.Value);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(i => i.Title.Contains(search) || i.Description.Contains(search) || i.Address.Contains(search));

            var issues = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return View(issues);
        }

        [HttpPost]
        [Route("Admin/Issues/BulkAction")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAction(string actionType, List<int> selectedIds, string? bulkAssigneeId, IssueStatus? bulkStatus)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một phản ánh!";
                return RedirectToAction(nameof(Index));
            }

            var issues = await _context.Issues.Where(i => selectedIds.Contains(i.Id)).ToListAsync();
            var adminId = _userManager.GetUserId(User) ?? "System";

            if (actionType == "assign" && !string.IsNullOrEmpty(bulkAssigneeId))
            {
                var staff = await _userManager.FindByIdAsync(bulkAssigneeId);
                foreach (var issue in issues)
                {
                    issue.AssignedToUserId = bulkAssigneeId;
                    issue.Status = IssueStatus.Assigned;
                    issue.AssignedAt = DateTime.UtcNow;

                    _context.IssueStatusHistories.Add(new IssueStatusHistory
                    {
                        IssueId = issue.Id,
                        FromStatus = issue.Status,
                        ToStatus = IssueStatus.Assigned,
                        ChangedById = adminId,
                        Note = $"Gán xử lý hàng loạt cho cán bộ {staff?.FullName}",
                        ChangedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã gán thành công {issues.Count} phản ánh!";
            }
            else if (actionType == "status" && bulkStatus.HasValue)
            {
                foreach (var issue in issues)
                {
                    var oldStatus = issue.Status;
                    issue.Status = bulkStatus.Value;

                    _context.IssueStatusHistories.Add(new IssueStatusHistory
                    {
                        IssueId = issue.Id,
                        FromStatus = oldStatus,
                        ToStatus = bulkStatus.Value,
                        ChangedById = adminId,
                        Note = "Cập nhật trạng thái hàng loạt từ trang quản trị",
                        ChangedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái của {issues.Count} phản ánh!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("Admin/Issues/ExportExcel")]
        public async Task<IActionResult> ExportExcel(IssueCategory? category, IssueStatus? status, IssuePriority? priority)
        {
            var query = _context.Issues.Include(i => i.Author).Include(i => i.AssignedUnit).AsQueryable();

            if (category.HasValue) query = query.Where(i => i.Category == category.Value);
            if (status.HasValue) query = query.Where(i => i.Status == status.Value);
            if (priority.HasValue) query = query.Where(i => i.Priority == priority.Value);

            var issues = await query.ToListAsync();

            // Export as CSV which opens directly in Excel with proper UTF-8 encoding
            var builder = new StringBuilder();
            builder.AppendLine("ID,Tieu De,Danh Muc,Dia Chi,Trang Thai,Do Khan,Ngay Gui,Han Xu Ly,Don Vi Giao");

            foreach (var item in issues)
            {
                builder.AppendLine($"#{item.Id},{item.Title.Replace(",", " ")},{item.Category},{item.Address.Replace(",", " ")},{item.Status},{item.Priority},{item.CreatedAt:yyyy-MM-dd HH:mm},{item.DueDate:yyyy-MM-dd HH:mm},{item.AssignedUnit?.Name}");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
            return File(bytes, "text/csv; charset=utf-8", $"Danh_sach_phan_anh_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        [Route("Admin/Issues/Kanban")]
        public async Task<IActionResult> Kanban()
        {
            var issues = await _context.Issues
                .Include(i => i.AssignedTo)
                .Include(i => i.Images)
                .ToListAsync();
            return View(issues);
        }

        [HttpPost]
        [Route("Admin/Issues/UpdateKanbanStatus")]
        public async Task<IActionResult> UpdateKanbanStatus(int issueId, IssueStatus newStatus)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null) return NotFound();

            var oldStatus = issue.Status;
            issue.Status = newStatus;

            var adminId = _userManager.GetUserId(User) ?? "System";
            _context.IssueStatusHistories.Add(new IssueStatusHistory
            {
                IssueId = issue.Id,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                ChangedById = adminId,
                Note = "Đang phân công",
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        [Route("Admin/Issues/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.Author)
                .Include(i => i.Images)
                .Include(i => i.Comments).ThenInclude(c => c.Author)
                .Include(i => i.StatusHistory).ThenInclude(h => h.ChangedBy)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null) return NotFound();

            ViewBag.Officials = await _userManager.Users.Where(u => u.GovernmentUnitId != null).ToListAsync();
            ViewBag.Units = await _context.GovernmentUnits.ToListAsync();
            ViewBag.RelatedIssues = await _context.Issues.Where(i => i.Id != id).Take(5).ToListAsync();

            return View(issue);
        }

        [HttpPost]
        [Route("Admin/Issues/UpdateDetails")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetails(int id, IssueStatus status, string? assignedToUserId, string? assignedUnitId, DateTime? dueDate, string? internalNotes, string? labels, string? resolutionDocumentUrl, string? resolutionImageUrl, string? note)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            var adminId = _userManager.GetUserId(User) ?? "System";
            var oldStatus = issue.Status;

            issue.Status = status;
            issue.AssignedToUserId = assignedToUserId;
            issue.AssignedUnitId = assignedUnitId;
            if (dueDate.HasValue) issue.DueDate = dueDate.Value;
            issue.InternalNotes = internalNotes;
            issue.Labels = labels;
            if (!string.IsNullOrEmpty(resolutionDocumentUrl)) issue.ResolutionDocumentUrl = resolutionDocumentUrl;
            if (!string.IsNullOrEmpty(resolutionImageUrl)) issue.ResolutionImageUrl = resolutionImageUrl;

            _context.IssueStatusHistories.Add(new IssueStatusHistory
            {
                IssueId = issue.Id,
                FromStatus = oldStatus,
                ToStatus = status,
                ChangedById = adminId,
                Note = note ?? "Cập nhật chi tiết xử lý từ quản trị viên",
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật chi tiết xử lý phản ánh thành công!";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpGet]
        [Route("Admin/Issues/RoutingRules")]
        public async Task<IActionResult> RoutingRules()
        {
            var rules = await _context.SmartRoutingRules
                .Include(r => r.TargetUnit)
                .ToListAsync();
            ViewBag.Units = await _context.GovernmentUnits.ToListAsync();
            return View(rules);
        }

        [HttpPost]
        [Route("Admin/Issues/CreateRoutingRule")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoutingRule(IssueCategory category, string? districtCode, string? wardCode, string targetUnitId, int slaDays)
        {
            var rule = new SmartRoutingRule
            {
                Category = category,
                DistrictCode = districtCode,
                WardCode = wardCode,
                TargetUnitId = targetUnitId,
                SLADays = slaDays,
                IsActive = true
            };

            _context.SmartRoutingRules.Add(rule);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã tạo quy tắc phân luồng thông minh thành công!";
            return RedirectToAction(nameof(RoutingRules));
        }

        [HttpPost]
        [Route("Admin/Issues/DeleteRoutingRule/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoutingRule(int id)
        {
            var rule = await _context.SmartRoutingRules.FindAsync(id);
            if (rule != null)
            {
                _context.SmartRoutingRules.Remove(rule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xoá quy tắc phân luồng thành công!";
            }
            return RedirectToAction(nameof(RoutingRules));
        }
    }
}

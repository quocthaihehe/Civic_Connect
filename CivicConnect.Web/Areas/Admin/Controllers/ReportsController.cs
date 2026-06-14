using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
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
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Admin/Reports")]
        public async Task<IActionResult> Index()
        {
            // Gather some numbers for standard reports summaries
            ViewBag.TotalCount = await _context.Issues.CountAsync();
            ViewBag.ResolvedCount = await _context.Issues.CountAsync(i => i.Status == IssueStatus.Resolved);
            ViewBag.SlaComplianceRate = 98.2; 
            
            return View();
        }

        [HttpPost]
        [Route("Admin/Reports/CustomQuery")]
        public async Task<IActionResult> CustomQuery(List<string> columns, IssueStatus? status, IssuePriority? priority, DateTime? fromDate, DateTime? toDate)
        {
            if (columns == null || !columns.Any())
            {
                columns = new List<string> { "Id", "Title", "Status", "Priority", "CreatedAt" };
            }

            var query = _context.Issues.AsQueryable();

            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);
            if (priority.HasValue)
                query = query.Where(i => i.Priority == priority.Value);
            if (fromDate.HasValue)
                query = query.Where(i => i.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(i => i.CreatedAt <= toDate.Value);

            var issues = await query.ToListAsync();

            // Format data dynamically based on requested columns for JSON response
            var resultsList = new List<Dictionary<string, string>>();

            foreach (var item in issues)
            {
                var dict = new Dictionary<string, string>();
                if (columns.Contains("Id")) dict["ID"] = $"#{item.Id}";
                if (columns.Contains("Title")) dict["Tiêu đề"] = item.Title;
                if (columns.Contains("Status")) dict["Trạng thái"] = item.Status.ToString();
                if (columns.Contains("Priority")) dict["Độ khẩn"] = item.Priority.ToString();
                if (columns.Contains("Address")) dict["Địa chỉ"] = item.Address;
                if (columns.Contains("CreatedAt")) dict["Ngày tạo"] = item.CreatedAt.ToString("dd/MM/yyyy");

                resultsList.Add(dict);
            }

            return Json(new { headers = resultsList.FirstOrDefault()?.Keys.ToList() ?? new List<string>(), rows = resultsList });
        }
    }
}

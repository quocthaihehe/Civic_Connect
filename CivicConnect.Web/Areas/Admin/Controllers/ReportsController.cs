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

        [HttpGet]
        [Route("Admin/Reports/ExportCsv")]
        public async Task<IActionResult> ExportCsv(DateTime? fromDate, DateTime? toDate, IssueCategory? category, IssueStatus? status)
        {
            var query = _context.Issues.Include(i => i.Author).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.CreatedAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(i => i.CreatedAt <= toDate.Value);
            if (category.HasValue)
                query = query.Where(i => i.Category == category.Value);
            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);

            var issues = await query.ToListAsync();

            var sb = new System.Text.StringBuilder();
            // Header: Id, Tiêu đề, Người gửi, Ngày gửi, Trạng thái, Phường/Quận
            sb.AppendLine("Id,Tiêu đề,Người gửi,Ngày gửi,Trạng thái,Phường/Quận");

            foreach (var issue in issues)
            {
                var id = issue.Id.ToString();
                var title = EscapeCsv(issue.Title);
                var authorName = issue.Author != null ? (!string.IsNullOrEmpty(issue.Author.FullName) ? issue.Author.FullName : issue.Author.UserName) : "";
                var author = EscapeCsv(authorName);
                var date = issue.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                var statusStr = EscapeCsv(issue.Status.ToString());
                
                var locationStr = "";
                if (!string.IsNullOrEmpty(issue.WardName) && !string.IsNullOrEmpty(issue.DistrictName))
                {
                    locationStr = $"{issue.WardName}, {issue.DistrictName}";
                }
                else if (!string.IsNullOrEmpty(issue.DistrictName))
                {
                    locationStr = issue.DistrictName;
                }
                else if (!string.IsNullOrEmpty(issue.WardName))
                {
                    locationStr = issue.WardName;
                }
                var location = EscapeCsv(locationStr);

                sb.AppendLine($"{id},{title},{author},{date},{statusStr},{location}");
            }

            // UTF8 with BOM for Excel compatibility
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", "BaoCaoPhanAnh.csv");
        }

        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            field = field.Replace("\"", "\"\"");
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field}\"";
            }
            return field;
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace CivicConnect.Web.Controllers
{
    public class MapController : Controller
    {
        private readonly AppDbContext _context;

        public MapController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["PageHeader"] = "Bản Đồ Phản Ánh";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetIssues()
        {
            var issues = await _context.Issues
                .Select(i => new
                {
                    i.Id,
                    i.Title,
                    i.Description,
                    Category = i.Category,
                    Status = i.Status,
                    i.Latitude,
                    i.Longitude,
                    i.Address
                })
                .ToListAsync();

            var result = issues.Select(i => new
            {
                i.Id,
                i.Title,
                i.Description,
                CategoryVal = (int)i.Category,
                CategoryName = i.Category switch
                {
                    IssueCategory.Traffic => "Giao thông",
                    IssueCategory.Environment => "Môi trường",
                    IssueCategory.Security => "An ninh trật tự",
                    IssueCategory.Infrastructure => "Hạ tầng công cộng",
                    IssueCategory.Administration => "Hành chính",
                    _ => "Khác"
                },
                CategoryText = i.Category.ToString(),
                StatusVal = (int)i.Status,
                StatusName = i.Status switch
                {
                    IssueStatus.Pending => "Chờ tiếp nhận",
                    IssueStatus.Assigned => "Đã phân công",
                    IssueStatus.Processing => "Đang xử lý",
                    IssueStatus.Resolved => "Đã giải quyết",
                    IssueStatus.Rejected => "Từ chối",
                    IssueStatus.Closed => "Đóng",
                    _ => i.Status.ToString()
                },
                StatusText = i.Status.ToString(),
                i.Latitude,
                i.Longitude,
                i.Address
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetHeatmapData()
        {
            var points = await _context.Issues
                .Where(i => i.Latitude != 0 && i.Longitude != 0)
                .Select(i => new
                {
                    lat = i.Latitude,
                    lng = i.Longitude,
                    // Intensity: ưu tiên cao = màu đậm hơn (Pending & Processing nóng nhất)
                    intensity = i.Status == IssueStatus.Pending ? 1.0
                              : i.Status == IssueStatus.Processing ? 0.8
                              : i.Status == IssueStatus.Assigned ? 0.6
                              : 0.3
                })
                .ToListAsync();

            return Json(points);
        }
    }
}

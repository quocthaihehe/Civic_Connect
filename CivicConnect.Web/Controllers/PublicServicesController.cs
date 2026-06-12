using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class PublicServicesController : Controller
    {
        private readonly AppDbContext _context;

        public PublicServicesController(AppDbContext context)
        {
            _context = context;
        }

        // Trang chủ Dịch vụ công
        public IActionResult Index()
        {
            return View();
        }

        // Trang Tra cứu thủ tục hành chính
        public async Task<IActionResult> Procedures(string search, string category)
        {
            var query = _context.AdministrativeProcedures.Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Code.Contains(search));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var procedures = await query.OrderBy(p => p.Title).ToListAsync();

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentCategory"] = category;
            
            // Get distinct categories for filter
            ViewBag.Categories = await _context.AdministrativeProcedures
                                        .Select(p => p.Category)
                                        .Distinct()
                                        .ToListAsync();

            return View(procedures);
        }

        // Trang Danh bạ cơ quan & Lịch tiếp dân
        public async Task<IActionResult> Directory(string search, string type)
        {
            var query = _context.AgencyDirectories.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Name.Contains(search) || a.Address.Contains(search));
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(a => a.Type == type);
            }

            var agencies = await query.OrderBy(a => a.OrderIndex).ThenBy(a => a.Name).ToListAsync();

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentType"] = type;

            // Get distinct types for filter
            ViewBag.Types = await _context.AgencyDirectories
                                        .Select(a => a.Type)
                                        .Distinct()
                                        .ToListAsync();

            return View(agencies);
        }
    }
}

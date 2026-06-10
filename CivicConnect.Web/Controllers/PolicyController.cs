using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CivicConnect.Infrastructure.Data;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class PolicyController : Controller
    {
        private readonly AppDbContext _context;

        public PolicyController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageHeader"] = "Chính Sách & Tin Tức";
            ViewData["Title"] = "Chính Sách & Tin Tức";
            var policies = await _context.Policies
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PublishedDate)
                .ToListAsync();
            return View(policies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var policy = await _context.Policies.FindAsync(id);
            if (policy == null || !policy.IsActive)
            {
                return NotFound();
            }

            ViewData["PageHeader"] = policy.Title;
            ViewData["Title"] = policy.Title;
            return View(policy);
        }
    }
}

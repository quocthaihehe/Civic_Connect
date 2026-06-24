using CivicConnect.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class LeaderboardController : Controller
    {
        private readonly AppDbContext _context;

        public LeaderboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var topCitizens = await _context.Users
                .OrderByDescending(u => u.CitizenPoints)
                .Take(20)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.UserName,
                    u.CitizenPoints,
                    u.TrustScore
                })
                .ToListAsync();

            return View(topCitizens);
        }
    }
}

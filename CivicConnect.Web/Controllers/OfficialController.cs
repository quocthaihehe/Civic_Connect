using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using CivicConnect.Web.Repositories;
using CivicConnect.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    [Authorize(Roles = "OfficialWard,OfficialDistrict,OfficialProvince,Admin")]
    public class OfficialController : Controller
    {
        private readonly IIssueService _issueService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfficialController(IIssueService issueService, UserManager<ApplicationUser> userManager)
        {
            _issueService = issueService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var issues = await _issueService.GetIssuesForOfficialAsync(userId);
            ViewData["PageHeader"] = "Bàn Làm Việc Cán Bộ";
            return View(issues);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var issue = await _issueService.GetIssueByIdAsync(id);
            if (issue == null) return NotFound();

            ViewData["PageHeader"] = "Xử Lý Phản Ánh";
            return View(issue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _issueService.AcceptIssueAsync(id, userId);
            if (result) TempData["SuccessMessage"] = "Đã tiếp nhận phản ánh thành công!";
            else TempData["ErrorMessage"] = "Không thể tiếp nhận phản ánh này.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _issueService.ProcessIssueAsync(id, userId);
            if (result) TempData["SuccessMessage"] = "Đã chuyển trạng thái sang Đang xử lý!";
            else TempData["ErrorMessage"] = "Không thể xử lý phản ánh này.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id, string? note, string? attachmentUrl)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _issueService.ResolveIssueAsync(id, userId, note, attachmentUrl);
            if (result) TempData["SuccessMessage"] = "Đã giải quyết xong phản ánh thành công!";
            else TempData["ErrorMessage"] = "Không thể giải quyết phản ánh này.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _issueService.RejectIssueAsync(id, userId, reason);
            if (result) TempData["SuccessMessage"] = "Đã từ chối phản ánh thành công.";
            else TempData["ErrorMessage"] = "Không thể từ chối phản ánh này.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Escalate(int id, string note)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _issueService.EscalateIssueAsync(id, userId, note);
            if (result) TempData["SuccessMessage"] = "Đã chuyển phản ánh lên cơ quan cấp Quận để tiếp tục xử lý!";
            else TempData["ErrorMessage"] = "Không thể chuyển cấp phản ánh này.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}

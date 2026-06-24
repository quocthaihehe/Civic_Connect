using CivicConnect.Web.Data;
using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public ProfileModel(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool IsPhoneVerified { get; set; }
        public CivicConnect.Web.Models.Enums.KYCLevel KYCLevel { get; set; }
        public int CitizenPoints { get; set; }
        public int TrustScore { get; set; }
        public List<PointTransaction> PointTransactions { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Họ tên là bắt buộc")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "CCCD/CMND")]
            public string? CitizenId { get; set; }

            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mã Tỉnh/Thành là bắt buộc")]
            [Display(Name = "Tỉnh/Thành")]
            public string ProvinceCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mã Quận/Huyện là bắt buộc")]
            [Display(Name = "Quận/Huyện")]
            public string DistrictCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mã Phường/Xã là bắt buộc")]
            [Display(Name = "Phường/Xã")]
            public string WardCode { get; set; } = string.Empty;

            [Display(Name = "Ảnh đại diện")]
            public string? AvatarUrl { get; set; }
            
            public string? IdCardFrontUrl { get; set; }
            public string? IdCardBackUrl { get; set; }
            public string? SelfieUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("./Login");

            IsPhoneVerified = user.IsPhoneVerified;
            KYCLevel = user.KYCLevel;
            CitizenPoints = user.CitizenPoints;
            TrustScore = user.TrustScore;

            PointTransactions = await _context.PointTransactions
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .ToListAsync();

            Input = new InputModel
            {
                FullName = user.FullName,
                CitizenId = user.CitizenId,
                PhoneNumber = user.PhoneNumber ?? "",
                ProvinceCode = user.ProvinceCode,
                DistrictCode = user.DistrictCode,
                WardCode = user.WardCode,
                AvatarUrl = user.AvatarUrl
            };
            ViewData["PageHeader"] = "Hồ Sơ Cá Nhân";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("./Login");

            IsPhoneVerified = user.IsPhoneVerified;

            if (ModelState.IsValid)
            {
                user.FullName = Input.FullName;
                user.CitizenId = Input.CitizenId;
                user.PhoneNumber = Input.PhoneNumber;
                user.ProvinceCode = Input.ProvinceCode;
                user.DistrictCode = Input.DistrictCode;
                user.WardCode = Input.WardCode;

                if (!string.IsNullOrEmpty(Input.AvatarUrl) && Input.AvatarUrl.StartsWith("https://"))
                {
                    user.AvatarUrl = Input.AvatarUrl;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToPage();
                }
                foreach (var err in result.Errors) ModelState.AddModelError(string.Empty, err.Description);
            }
            ViewData["PageHeader"] = "Hồ Sơ Cá Nhân";
            return Page();
        }

        public async Task<IActionResult> OnPostSubmitKycAsync([FromForm] string idCardFrontUrl, [FromForm] string idCardBackUrl, [FromForm] string selfieUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("./Login");

            if (string.IsNullOrEmpty(idCardFrontUrl) || string.IsNullOrEmpty(idCardBackUrl) || string.IsNullOrEmpty(selfieUrl))
            {
                TempData["ErrorMessage"] = "Vui lòng tải lên đầy đủ 3 ảnh bắt buộc.";
                return RedirectToPage();
            }

            user.IdCardFrontUrl = idCardFrontUrl;
            user.IdCardBackUrl = idCardBackUrl;
            user.SelfieUrl = selfieUrl;
            user.KYCLevel = CivicConnect.Web.Models.Enums.KYCLevel.PendingReview;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đã nộp hồ sơ xác thực danh tính. Vui lòng chờ Admin phê duyệt.";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi nộp hồ sơ KYC.";
            }

            return RedirectToPage();
        }
    }
}

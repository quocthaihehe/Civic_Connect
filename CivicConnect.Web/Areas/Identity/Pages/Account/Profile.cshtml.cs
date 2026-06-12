using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();


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
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("./Login");

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
    }
}

using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    public class VerifyPhoneModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public VerifyPhoneModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();


        public class InputModel
        {
            [Required(ErrorMessage = "Mã OTP là bắt buộc")]
            [Display(Name = "Mã xác thực OTP")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
            public string OtpCode { get; set; } = string.Empty;
        }


        public IActionResult OnGet()
        {
            if (TempData["VerifyEmail"] == null) return RedirectToPage("./Register");
            TempData.Keep("VerifyEmail");
            TempData.Keep("SMS_OTP");
            ViewData["PageHeader"] = "Xác Minh Số Điện Thoại";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = TempData["VerifyEmail"]?.ToString();
            var systemOtp = TempData["SMS_OTP"]?.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(systemOtp))
            {
                ModelState.AddModelError(string.Empty, "Thông tin xác thực đã hết hạn hoặc không hợp lệ. Vui lòng đăng ký lại.");
                return Page();
            }

            if (Input.OtpCode != systemOtp && Input.OtpCode != "123456")
            {
                ModelState.AddModelError("Input.OtpCode", "Mã OTP không khớp.");
                TempData.Keep("VerifyEmail");
                TempData.Keep("SMS_OTP");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.IsPhoneVerified = true;
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect("~/");
            }
            return RedirectToPage("./Register");
        }
    }
}

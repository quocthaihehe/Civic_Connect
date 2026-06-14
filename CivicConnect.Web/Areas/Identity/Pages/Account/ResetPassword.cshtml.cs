using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Mã OTP là bắt buộc")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
            [Display(Name = "Mã xác thực OTP")]
            public string OtpCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải dài ít nhất 8 ký tự")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string NewPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu mới")]
            [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận mật khẩu không trùng khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            if (TempData["ResetUserId"] == null)
            {
                return RedirectToPage("./ForgotPassword");
            }

            // Giữ lại TempData để dùng cho OnPost
            TempData.Keep("ResetUserId");
            TempData.Keep("ResetMethod");
            TempData.Keep("ResetContact");
            TempData.Keep("ResetOTP");

            ViewData["PageHeader"] = "Đặt Lại Mật Khẩu";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = TempData["ResetUserId"]?.ToString();
            var systemOtp = TempData["ResetOTP"]?.ToString();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(systemOtp))
            {
                ModelState.AddModelError(string.Empty, "Yêu cầu khôi phục đã hết hạn. Vui lòng thực hiện lại từ đầu.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                TempData.Keep("ResetUserId");
                TempData.Keep("ResetMethod");
                TempData.Keep("ResetContact");
                TempData.Keep("ResetOTP");
                return Page();
            }

            // Kiểm tra mã OTP (cho phép 123456 làm mã mặc định khi test)
            if (Input.OtpCode != systemOtp && Input.OtpCode != "123456")
            {
                ModelState.AddModelError("Input.OtpCode", "Mã OTP không chính xác.");
                TempData.Keep("ResetUserId");
                TempData.Keep("ResetMethod");
                TempData.Keep("ResetContact");
                TempData.Keep("ResetOTP");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin tài khoản người dùng.");
                TempData.Keep("ResetUserId");
                TempData.Keep("ResetMethod");
                TempData.Keep("ResetContact");
                TempData.Keep("ResetOTP");
                return Page();
            }

            // Sử dụng Identity Token Provider để reset mật khẩu an toàn
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập với mật khẩu mới.";
                return RedirectToPage("./Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData.Keep("ResetUserId");
            TempData.Keep("ResetMethod");
            TempData.Keep("ResetContact");
            TempData.Keep("ResetOTP");
            return Page();
        }
    }
}

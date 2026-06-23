using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    [Authorize]
    public class Profile2FAModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UrlEncoder _urlEncoder;

        public Profile2FAModel(UserManager<ApplicationUser> userManager, UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _urlEncoder = urlEncoder;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool TwoFactorEnabled { get; set; }
        public string? CurrentTwoFactorType { get; set; }
        public string? CurrentContact { get; set; }
        
        public string? AuthenticatorKey { get; set; }
        public string? AuthenticatorUri { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Bật xác thực 2FA")]
            public bool Enable2FA { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn loại xác thực 2FA")]
            [Display(Name = "Phương thức 2FA")]
            public string TwoFactorType { get; set; } = "Telegram";

            [Display(Name = "Địa chỉ liên hệ / ID (Telegram ChatID, Discord Webhook,...)")]
            public string? ContactInfo { get; set; }

            [Display(Name = "Mã xác thực")]
            public string? VerificationCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            TwoFactorEnabled = user.TwoFactorEnabledCustom;
            CurrentTwoFactorType = user.TwoFactorType;
            CurrentContact = user.TwoFactorContact;

            Input = new InputModel
            {
                Enable2FA = user.TwoFactorEnabledCustom,
                TwoFactorType = user.TwoFactorType ?? "Telegram",
                ContactInfo = user.TwoFactorContact
            };

            await LoadSharedKeyAndQrCodeUriAsync(user);

            ViewData["PageHeader"] = "Bảo mật hai lớp (2FA)";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            TwoFactorEnabled = user.TwoFactorEnabledCustom;
            CurrentTwoFactorType = user.TwoFactorType;
            CurrentContact = user.TwoFactorContact;

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                ViewData["PageHeader"] = "Bảo mật hai lớp (2FA)";
                return Page();
            }

            if (Input.Enable2FA && Input.TwoFactorType == "Authenticator")
            {
                if (string.IsNullOrEmpty(Input.VerificationCode))
                {
                    ModelState.AddModelError("Input.VerificationCode", "Vui lòng nhập mã xác nhận từ ứng dụng Authenticator để hoàn tất thiết lập.");
                    await LoadSharedKeyAndQrCodeUriAsync(user);
                    ViewData["PageHeader"] = "Bảo mật hai lớp (2FA)";
                    return Page();
                }

                var verificationCode = Input.VerificationCode.Replace(" ", "").Replace("-", "");
                var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

                if (!is2faTokenValid)
                {
                    ModelState.AddModelError("Input.VerificationCode", "Mã xác nhận không hợp lệ. Vui lòng thử lại.");
                    await LoadSharedKeyAndQrCodeUriAsync(user);
                    ViewData["PageHeader"] = "Bảo mật hai lớp (2FA)";
                    return Page();
                }
            }

            user.TwoFactorEnabledCustom = Input.Enable2FA;
            user.TwoFactorType = Input.TwoFactorType;
            user.TwoFactorContact = Input.ContactInfo;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật cấu hình bảo mật 2FA thành công!";
                return RedirectToPage();
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            await LoadSharedKeyAndQrCodeUriAsync(user);
            ViewData["PageHeader"] = "Bảo mật hai lớp (2FA)";
            return Page();
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            AuthenticatorKey = unformattedKey;

            var email = await _userManager.GetEmailAsync(user);
            AuthenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string format = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
            return string.Format(
                format,
                _urlEncoder.Encode("CivicConnect"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}

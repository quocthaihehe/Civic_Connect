using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    [Authorize]
    public class Profile2FAModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Profile2FAModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool TwoFactorEnabled { get; set; }
        public string? CurrentTwoFactorType { get; set; }
        public string? CurrentContact { get; set; }
        public string? AuthenticatorSecret { get; set; }

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
            AuthenticatorSecret = user.TwoFactorSecret;

            Input = new InputModel
            {
                Enable2FA = user.TwoFactorEnabledCustom,
                TwoFactorType = user.TwoFactorType ?? "Telegram",
                ContactInfo = user.TwoFactorContact
            };

            if (string.IsNullOrEmpty(AuthenticatorSecret))
            {
                AuthenticatorSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper();
            }

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

            if (ModelState.IsValid)
            {
                user.TwoFactorEnabledCustom = Input.Enable2FA;
                user.TwoFactorType = Input.TwoFactorType;
                user.TwoFactorContact = Input.ContactInfo;

                if (Input.Enable2FA && Input.TwoFactorType == "Authenticator" && string.IsNullOrEmpty(user.TwoFactorSecret))
                {
                    // Generate new secret for Authenticator
                    user.TwoFactorSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper();
                }

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
            }

            ViewData["PageHeader"] = "Bảo mật hai lớp (2FA)";
            return Page();
        }
    }
}

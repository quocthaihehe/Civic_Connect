using CivicConnect.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Lỗi từ Google: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Không thể lấy thông tin đăng nhập từ Google.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} đã đăng nhập thành công bằng tài khoản {LoginProvider}.", info.Principal.Identity?.Name, info.LoginProvider);
                
                // Update online status
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    user.IsOnline = true;
                    await _userManager.UpdateAsync(user);
                }
                
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, check if they exist by email.
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    ErrorMessage = "Không thể lấy địa chỉ Email từ tài khoản Google của bạn.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Create a new user with Google claims!
                    var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Thành viên CivicConnect";
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        IsEmailVerified = true,
                        EmailConfirmed = true,
                        IsActive = true,
                        ProvinceCode = "79",      // TP. Hồ Chí Minh
                        DistrictCode = "760",    // Quận 1
                        WardCode = "26734",      // Phường Bến Nghé
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow,
                        IsOnline = true
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Citizen");
                        createResult = await _userManager.AddLoginAsync(user, info);
                        if (createResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                            _logger.LogInformation("Tạo tài khoản liên kết Google thành công cho {Email}.", email);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    
                    foreach (var error in createResult.Errors)
                    {
                        _logger.LogError("Lỗi tạo tài khoản liên kết Google: {Error}", error.Description);
                    }
                    
                    ErrorMessage = "Đã xảy ra lỗi khi tạo tài khoản liên kết Google.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
                else
                {
                    // User exists but hasn't linked Google. Link it now!
                    var linkResult = await _userManager.AddLoginAsync(user, info);
                    if (linkResult.Succeeded)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        user.IsOnline = true;
                        await _userManager.UpdateAsync(user);
                        
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        _logger.LogInformation("Liên kết tài khoản Google thành công cho {Email}.", email);
                        return LocalRedirect(returnUrl);
                    }
                    
                    ErrorMessage = "Tài khoản email này đã được đăng ký nhưng không thể liên kết với Google.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }
        }
    }
}

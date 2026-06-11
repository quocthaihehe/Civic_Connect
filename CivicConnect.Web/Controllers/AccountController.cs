using CivicConnect.Core.Entities;
using CivicConnect.Core.Interfaces;
using CivicConnect.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CivicConnect.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPhotoService _photoService;
        private readonly ISmsService _smsService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPhotoService photoService,
            ISmsService smsService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _photoService = photoService;
            _smsService = smsService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["PageHeader"] = "Đăng Ký Tài Khoản";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    ProvinceCode = model.ProvinceCode,
                    DistrictCode = model.DistrictCode,
                    WardCode = model.WardCode,
                    IsEmailVerified = true, // Tự động xác minh cho môi trường phát triển
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Gán vai trò Công dân mặc định
                    await _userManager.AddToRoleAsync(user, "Citizen");

                    // Tạo mã OTP ngẫu nhiên (hoặc mặc định 123456 để test nhanh)
                    var otp = new Random().Next(100000, 999999).ToString();
                    TempData["SMS_OTP"] = otp;
                    TempData["VerifyEmail"] = model.Email;

                    // Gửi OTP qua SMS Mock
                    await _smsService.SendSmsAsync(user.PhoneNumber, $"Mã OTP xác thực tài khoản CivicConnect của bạn là: {otp}. Hạn dùng 5 phút.");

                    return RedirectToAction(nameof(VerifyPhone));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["PageHeader"] = "Đăng Ký Tài Khoản";
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyPhone()
        {
            if (TempData["VerifyEmail"] == null)
            {
                return RedirectToAction(nameof(Register));
            }
            // Giữ lại TempData cho bước POST tiếp theo
            TempData.Keep("VerifyEmail");
            TempData.Keep("SMS_OTP");
            
            ViewData["PageHeader"] = "Xác Minh Số Điện Thoại";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPhone(VerifyPhoneViewModel model)
        {
            var email = TempData["VerifyEmail"]?.ToString();
            var systemOtp = TempData["SMS_OTP"]?.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(systemOtp))
            {
                ModelState.AddModelError(string.Empty, "Thông tin xác thực đã hết hạn hoặc không hợp lệ. Vui lòng đăng ký lại.");
                return View(model);
            }

            if (model.OtpCode != systemOtp && model.OtpCode != "123456") // Cho phép OTP mặc định 123456 để tester dễ sử dụng
            {
                ModelState.AddModelError(nameof(model.OtpCode), "Mã OTP không khớp.");
                TempData.Keep("VerifyEmail");
                TempData.Keep("SMS_OTP");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.IsPhoneVerified = true;
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);

                // Đăng nhập người dùng luôn
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Register));
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["PageHeader"] = "Đăng Nhập";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa tạm thời do nhập sai nhiều lần. Hãy quay lại sau 15 phút.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                }
            }

            ViewData["PageHeader"] = "Đăng Nhập";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ProfileViewModel
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
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName;
                user.CitizenId = model.CitizenId;
                user.PhoneNumber = model.PhoneNumber;
                user.ProvinceCode = model.ProvinceCode;
                user.DistrictCode = model.DistrictCode;
                user.WardCode = model.WardCode;

                // Avatar được upload trực tiếp lên Cloudinary từ trình duyệt (client-side)
                // Server chỉ nhận lại URL string
                if (!string.IsNullOrEmpty(model.AvatarUrl) && model.AvatarUrl.StartsWith("https://"))
                {
                    user.AvatarUrl = model.AvatarUrl;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
            }

            ViewData["PageHeader"] = "Hồ Sơ Cá Nhân";
            return View(model);
        }
    }
}

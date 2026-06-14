using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace CivicConnect.Web.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ISmsService smsService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _smsService = smsService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng chọn phương thức khôi phục")]
            public string RecoveryMethod { get; set; } = "Email"; // "Email" or "Phone"

            [Required(ErrorMessage = "Vui lòng nhập thông tin liên hệ")]
            public string Contact { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            ViewData["PageHeader"] = "Quên Mật Khẩu";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            ApplicationUser? user = null;

            if (Input.RecoveryMethod == "Email")
            {
                user = await _userManager.FindByEmailAsync(Input.Contact.Trim());
                if (user == null)
                {
                    ModelState.AddModelError("Input.Contact", "Địa chỉ Gmail này không tồn tại trên hệ thống.");
                    return Page();
                }
            }
            else // Phone
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == Input.Contact.Trim());
                if (user == null)
                {
                    ModelState.AddModelError("Input.Contact", "Số điện thoại này chưa được đăng ký.");
                    return Page();
                }
            }

            // Tạo mã OTP ngẫu nhiên 6 chữ số
            var otpCode = Random.Shared.Next(100000, 999999).ToString();

            // Lưu thông tin vào TempData để chuyển sang bước nhập OTP và Reset mật khẩu
            TempData["ResetUserId"] = user.Id;
            TempData["ResetMethod"] = Input.RecoveryMethod;
            TempData["ResetContact"] = Input.Contact.Trim();
            TempData["ResetOTP"] = otpCode;

            // Gửi OTP theo phương thức đã chọn
            if (Input.RecoveryMethod == "Email")
            {
                try
                {
                    var subject = "[CivicConnect] Mã xác thực khôi phục mật khẩu";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                            <div style='text-align: center; margin-bottom: 20px;'>
                                <h2 style='color: #1b4d3e; margin: 0; font-size: 28px; font-weight: bold;'>CivicConnect</h2>
                                <span style='font-size: 12px; color: #666; letter-spacing: 1px; text-transform: uppercase;'>Cổng thông tin phản ánh hiện trường</span>
                            </div>
                            <p>Xin chào,</p>
                            <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <span style='font-size: 26px; font-weight: bold; letter-spacing: 6px; padding: 12px 24px; background-color: #f0fdf4; border: 1px dashed #1b4d3e; border-radius: 6px; color: #1b4d3e; display: inline-block;'>{otpCode}</span>
                            </div>
                            <p>Mã xác thực OTP này có giá trị trong vòng 10 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
                            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                            <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;'>
                            <p style='font-size: 11px; color: #888888; text-align: center;'>Hệ thống Phản Ánh Hiện Trường CivicConnect &copy; 2026</p>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email!, subject, body);
                    TempData["SuccessMessage"] = $"Mã OTP đã được gửi đến hòm thư {user.Email}. Vui lòng kiểm tra email.";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi gửi email: {ex.Message}");
                    return Page();
                }
            }
            else // Phone
            {
                var message = $"[CivicConnect] Ma OTP khoi phuc mat khau cua ban la {otpCode}. Khong chia se ma nay cho bat ky ai.";
                await _smsService.SendSmsAsync(user.PhoneNumber!, message);
                TempData["SuccessMessage"] = $"Mã OTP đã được gửi qua tin nhắn SMS tới số {user.PhoneNumber}.";
            }

            return RedirectToPage("./ResetPassword");
        }
    }
}

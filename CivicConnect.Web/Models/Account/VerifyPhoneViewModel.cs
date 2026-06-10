using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Account
{
    public class VerifyPhoneViewModel
    {
        [Required(ErrorMessage = "Mã xác thực là bắt buộc.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải bao gồm 6 chữ số.")]
        [Display(Name = "Mã OTP (6 chữ số)")]
        public string OtpCode { get; set; }
    }
}

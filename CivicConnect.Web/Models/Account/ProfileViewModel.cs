using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Account
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "CCCD/CMND")]
        public string? CitizenId { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Mã phường/xã")]
        public string? WardCode { get; set; }

        [Display(Name = "Mã quận/huyện")]
        public string? DistrictCode { get; set; }

        [Display(Name = "Mã tỉnh/thành")]
        public string? ProvinceCode { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Tải ảnh đại diện mới")]
        public IFormFile? AvatarFile { get; set; }
    }
}

using CivicConnect.Core.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CivicConnect.Web.Models.Issues
{
    public class IssueCreateViewModel
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tiêu đề phải từ 5 đến 200 ký tự.")]
        [Display(Name = "Tiêu đề phản ánh")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả chi tiết là bắt buộc.")]
        [StringLength(5000, MinimumLength = 20, ErrorMessage = "Mô tả phải từ 20 đến 5000 ký tự.")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục phản ánh.")]
        [Display(Name = "Danh mục")]
        public IssueCategory Category { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mức độ ưu tiên đề xuất.")]
        [Display(Name = "Mức độ ưu tiên đề xuất")]
        public IssuePriority Priority { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vị trí trên bản đồ.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vị trí trên bản đồ.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập hoặc chọn địa chỉ.")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string Address { get; set; }

        public string WardCode { get; set; }
        public string WardName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceCode { get; set; }
        public string ProvinceName { get; set; }

        [Display(Name = "Gửi ẩn danh (không hiện tên của bạn công khai)")]
        public bool IsAnonymous { get; set; }

        [Display(Name = "Đính kèm hình ảnh (tối đa 5 ảnh, max 5MB/ảnh)")]
        public List<IFormFile>? ImageFiles { get; set; }
    }
}

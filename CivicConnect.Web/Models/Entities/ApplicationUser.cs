using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace CivicConnect.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }          // Họ tên thật
        public string? CitizenId { get; set; }        // CCCD/CMND (optional)
        public bool IsPhoneVerified { get; set; }     // Đã xác thực SĐT chưa
        public bool IsEmailVerified { get; set; }     // Đã xác thực email chưa
        public string? AvatarUrl { get; set; }        // URL ảnh đại diện (Cloudinary)
        public string? WardCode { get; set; }         // Mã phường/xã cư trú
        public string? DistrictCode { get; set; }     // Mã quận/huyện
        public string? ProvinceCode { get; set; }     // Mã tỉnh/thành
        public bool IsActive { get; set; } = true;    // Khoá tài khoản
        public int TrustScore { get; set; } = 0;      // Điểm uy tín
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // Nâng cấp: KYC & Vi phạm & Trực tuyến
        public int CitizenPoints { get; set; } = 100; // Điểm công dân mặc định
        public CivicConnect.Web.Models.Enums.KYCLevel KYCLevel { get; set; } = CivicConnect.Web.Models.Enums.KYCLevel.Unverified;
        public string? IdCardFrontUrl { get; set; }
        public string? IdCardBackUrl { get; set; }
        public string? SelfieUrl { get; set; }
        public bool IsRestricted { get; set; } = false;
        public string? RestrictionReason { get; set; }
        public DateTime? RestrictedUntil { get; set; }
        public bool IsOnline { get; set; } = false;

        // Custom 2FA fields
        public bool TwoFactorEnabledCustom { get; set; } = false;
        public string? TwoFactorType { get; set; } // "Telegram", "Discord", "Authenticator"
        public string? TwoFactorSecret { get; set; }
        public string? TwoFactorContact { get; set; }

        public string BadgeLevel => TrustScore switch
        {
            >= 500 => "Kim cương",
            >= 200 => "Vàng",
            >= 50 => "Bạc",
            >= 10 => "Đồng",
            _ => "Tân binh"
        };

        // Liên kết cán bộ với đơn vị cơ quan hành chính
        public string? GovernmentUnitId { get; set; }
        public GovernmentUnit? GovernmentUnit { get; set; }

        // Navigation
        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}

using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace CivicConnect.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueImage> IssueImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<IssueStatusHistory> IssueStatusHistories { get; set; }
        public DbSet<GovernmentUnit> GovernmentUnits { get; set; }
        public DbSet<UnitCategory> UnitCategories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<DonationCategory> DonationCategories { get; set; }
        public DbSet<Donation> Donations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng biểu quyết Vote
            modelBuilder.Entity<Vote>(entity =>
            {
                // Một user chỉ vote tối đa 1 lần cho mỗi issue (up hoặc down)
                entity.HasIndex(v => new { v.UserId, v.IssueId }).IsUnique();
                
                // Restrict cascade delete từ User
                entity.HasOne<ApplicationUser>()
                    .WithMany(u => u.Votes)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Cascade delete từ Issue
                entity.HasOne<Issue>()
                    .WithMany(i => i.Votes)
                    .HasForeignKey(v => v.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng phản ánh Issue
            modelBuilder.Entity<Issue>(entity =>
            {
                // Author relationship - Restrict cascade
                entity.HasOne(i => i.Author)
                    .WithMany(u => u.Issues)
                    .HasForeignKey(i => i.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // AssignedTo relationship - Restrict cascade
                entity.HasOne(i => i.AssignedTo)
                    .WithMany()
                    .HasForeignKey(i => i.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // AssignedUnit relationship - Restrict cascade
                entity.HasOne(i => i.AssignedUnit)
                    .WithMany(gu => gu.AssignedIssues)
                    .HasForeignKey(i => i.AssignedUnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Tự liên kết để Escalation (Chuyển cấp)
                entity.HasOne(i => i.ParentIssue)
                    .WithMany(i => i.ChildIssues)
                    .HasForeignKey(i => i.ParentIssueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình bình luận Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Issue)
                    .WithMany(i => i.Comments)
                    .HasForeignKey(c => c.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình lịch sử trạng thái IssueStatusHistory
            modelBuilder.Entity<IssueStatusHistory>(entity =>
            {
                entity.HasOne(h => h.ChangedBy)
                    .WithMany()
                    .HasForeignKey(h => h.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Issue)
                    .WithMany(i => i.StatusHistory)
                    .HasForeignKey(h => h.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình đơn vị cơ quan GovernmentUnit
            modelBuilder.Entity<GovernmentUnit>(entity =>
            {
                entity.HasOne(gu => gu.ParentUnit)
                    .WithMany(gu => gu.ChildUnits)
                    .HasForeignKey(gu => gu.ParentUnitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình bảng phụ thẩm quyền UnitCategory
            modelBuilder.Entity<UnitCategory>(entity =>
            {
                entity.HasKey(uc => new { uc.GovernmentUnitId, uc.Category });

                entity.HasOne(uc => uc.GovernmentUnit)
                    .WithMany(gu => gu.Categories)
                    .HasForeignKey(uc => uc.GovernmentUnitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed các vai trò Identity (Roles)
            var roles = new[] { "Admin", "OfficialProvince", "OfficialDistrict", "OfficialWard", "DepartmentStaff", "Citizen" };
            foreach (var roleName in roles)
            {
                modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
                {
                    Id = roleName,
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }

            // Seed đơn vị cơ quan hành chính Quận 1 và Phường Bến Nghé (Mã chuẩn VN)
            modelBuilder.Entity<GovernmentUnit>().HasData(
                new GovernmentUnit
                {
                    Id = "UBND_Q1",
                    Name = "UBND Quận 1",
                    Type = GovernmentUnitType.District,
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    Address = "47 Lê Duẩn, Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    Phone = "02838279789",
                    Email = "quan1@tphcm.gov.vn"
                },
                new GovernmentUnit
                {
                    Id = "UBND_BENNGHE",
                    Name = "UBND Phường Bến Nghé",
                    Type = GovernmentUnitType.Ward,
                    ParentUnitId = "UBND_Q1",
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    WardCode = "26734",
                    Address = "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    Phone = "02838290290",
                    Email = "bennghe.q1@tphcm.gov.vn"
                }
            );

            // Seed thẩm quyền cơ quan xử lý theo danh mục
            modelBuilder.Entity<UnitCategory>().HasData(
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Traffic },
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Environment },
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Security },
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Infrastructure },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Traffic },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Environment },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Security },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Infrastructure },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Administration }
            );

            // Seed sample policies/announcements with legal metadata
            modelBuilder.Entity<Policy>().HasData(
                new Policy
                {
                    Id = 1,
                    Title = "Nghị định 45/2026/NĐ-CP về xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị",
                    Excerpt = "Chính phủ ban hành quy định tăng mức phạt đối với các hành vi xả rác bừa bãi, đổ chất thải không đúng nơi quy định và lấn chiếm lòng lề đường.",
                    Content = "Nghị định quy định chi tiết các mức xử phạt đối với cá nhân, tổ chức có hành vi vi phạm vệ sinh môi trường đô thị. Mức phạt tiền tối đa đối với cá nhân là 1.000.000đ cho hành vi vứt rác không đúng nơi quy định, 5.000.000đ cho hành vi tự ý đổ rác thải sinh hoạt ra lòng đường, vỉa hè. Các đơn vị kinh doanh lấn chiếm vỉa hè sẽ bị xử phạt từ 10.000.000đ đến 20.000.000đ và buộc khôi phục tình trạng ban đầu.",
                    Tag = "Nghị định",
                    TagClass = "tag-law",
                    IssuingUnit = "Chính phủ",
                    PublishedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "45/2026/NĐ-CP",
                    DocumentType = "Nghị định",
                    Signer = "Thủ tướng Phạm Minh Chính",
                    SourceUrl = "https://vanban.chinhphu.vn",
                    EffectiveDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Policy
                {
                    Id = 2,
                    Title = "Thông tư 08/2026/TT-BXD hướng dẫn về chỉnh trang đô thị và phát triển không gian công cộng xanh",
                    Excerpt = "Bộ Xây dựng ban hành hướng dẫn thực hiện các chỉ tiêu về diện tích cây xanh, hoa và hạ tầng tiện ích tại khu dân cư đô thị.",
                    Content = "Bộ Xây dựng ban hành Thông tư hướng dẫn chi tiết quy chuẩn kỹ thuật quốc gia về quy hoạch xây dựng. Trong đó, yêu cầu các khu dân cư mới phải đạt tỷ lệ diện tích cây xanh tối thiểu là 2m2/người, khuyến khích các khu dân cư hiện hữu tận dụng các ngõ hẻm để trồng hoa, cây cảnh công cộng và tạo không gian sinh hoạt cộng đồng tự quản.",
                    Tag = "Thông tư",
                    TagClass = "tag-policy",
                    IssuingUnit = "Bộ Xây dựng",
                    PublishedDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "08/2026/TT-BXD",
                    DocumentType = "Thông tư",
                    Signer = "Bộ trưởng Nguyễn Thanh Nghị",
                    SourceUrl = "https://moc.gov.vn",
                    EffectiveDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Policy
                {
                    Id = 3,
                    Title = "Thông báo 124/TB-UBND ra quân tổng vệ sinh môi trường trên địa bàn Phường Bến Nghé",
                    Excerpt = "UBND Phường phát động lễ ra quân tổng vệ sinh, dọn dẹp rác thải và trang trí tuyến hẻm xanh vào sáng Chủ Nhật.",
                    Content = "Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung, UBND Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ. Thời gian bắt đầu từ 7:30 sáng ngày Chủ Nhật.",
                    Tag = "Thông báo",
                    TagClass = "tag-notice",
                    IssuingUnit = "UBND Phường Bến Nghé",
                    PublishedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "124/TB-UBND",
                    DocumentType = "Thông báo",
                    Signer = "Chủ tịch UBND Phường",
                    SourceUrl = "",
                    EffectiveDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Policy
                {
                    Id = 4,
                    Title = "Hơn 500 đoàn viên thanh niên Quận 1 tham gia làm sạch kênh Nhiêu Lộc - Thị Nghè",
                    Excerpt = "Chiến dịch 'Chung tay bảo vệ dòng sông quê hương' thu hút hàng trăm bạn trẻ dọn dẹp rác thải nhựa và vớt lục bình.",
                    Content = "Sáng nay, Quận Đoàn 1 phối hợp với Công ty Môi trường Đô thị tổ chức lễ phát động ra quân làm sạch toàn tuyến kênh chảy qua địa bàn quận. Sau 4 giờ làm việc nỗ lực, lực lượng tình nguyện đã thu gom được hơn 3 tấn rác thải các loại, chủ yếu là túi ni lông, chai nhựa và rác sinh hoạt bị vứt xuống lòng kênh. Đây là hoạt động thường niên nhằm tuyên truyền ý thức bảo vệ môi trường nước cho cư dân xung quanh.",
                    Tag = "Tin tức",
                    TagClass = "tag-news",
                    IssuingUnit = "Quận Đoàn 1",
                    PublishedDate = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "",
                    DocumentType = "Tin tức",
                    Signer = "",
                    SourceUrl = "https://tuoitre.vn",
                    EffectiveDate = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Cấu hình bảng DonationCategory
            modelBuilder.Entity<DonationCategory>(entity =>
            {
                entity.Property(dc => dc.TargetAmount).HasColumnType("decimal(18,2)");
                entity.Property(dc => dc.CurrentAmount).HasColumnType("decimal(18,2)");
            });

            // Cấu hình bảng Donation
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.Property(d => d.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(d => d.OrderId).IsUnique();

                entity.HasOne(d => d.DonationCategory)
                    .WithMany(dc => dc.Donations)
                    .HasForeignKey(d => d.DonationCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed các Quỹ Quyên Góp mẫu
            modelBuilder.Entity<DonationCategory>().HasData(
                new DonationCategory
                {
                    Id = 1,
                    Name = "Quỹ Trồng Xanh Đô Thị",
                    Description = "Quyên góp mua cây xanh, hoa trang trí trồng tại các tuyến ngõ hẻm, công viên công cộng trên địa bàn Phường Bến Nghé.",
                    TargetAmount = 50000000,
                    CurrentAmount = 0,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new DonationCategory
                {
                    Id = 2,
                    Name = "Quỹ Thắp Sáng Ngõ Hẻm",
                    Description = "Hỗ trợ lắp đặt hệ thống đèn đường LED thông minh, tiết kiệm điện tại các ngõ hẻm chưa có đủ ánh sáng.",
                    TargetAmount = 30000000,
                    CurrentAmount = 0,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new DonationCategory
                {
                    Id = 3,
                    Name = "Quỹ Nâng Cấp Sân Chơi Trẻ Em",
                    Description = "Mua sắm, lắp đặt và sửa chữa các thiết bị vui chơi ngoài trời tại điểm sinh hoạt cộng đồng của phường.",
                    TargetAmount = 100000000,
                    CurrentAmount = 0,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}

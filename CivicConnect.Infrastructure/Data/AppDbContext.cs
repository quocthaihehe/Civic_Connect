using CivicConnect.Core.Entities;
using CivicConnect.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace CivicConnect.Infrastructure.Data
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

            // Seed sample policies/announcements
            modelBuilder.Entity<Policy>().HasData(
                new Policy
                {
                    Id = 1,
                    Title = "Nghị định mới về xử phạt hành chính vi phạm môi trường đô thị",
                    Excerpt = "Chính phủ vừa ban hành nghị định mới tăng mức phạt đối với các hành vi xả rác bừa bãi và lấn chiếm lòng lề đường tại đô thị.",
                    Content = "Nội dung chi tiết nghị định mới...",
                    Tag = "Luật mới",
                    TagClass = "tag-law",
                    IssuingUnit = "UBND Quận 1",
                    PublishedDate = DateTime.UtcNow.AddDays(-2),
                    IsActive = true
                },
                new Policy
                {
                    Id = 2,
                    Title = "Thông báo ra quân dọn dẹp vệ sinh môi trường trên địa bàn Phường Bến Nghé",
                    Excerpt = "Thực hiện nếp sống văn minh đô thị, UBND Phường tổ chức đợt ra quân tổng vệ sinh các tuyến đường trọng điểm vào sáng Chủ Nhật tuần này.",
                    Content = "Nội dung thông báo chi tiết...",
                    Tag = "Thông báo",
                    TagClass = "tag-notice",
                    IssuingUnit = "UBND Phường Bến Nghé",
                    PublishedDate = DateTime.UtcNow.AddDays(-1),
                    IsActive = true
                },
                new Policy
                {
                    Id = 3,
                    Title = "Chính sách hỗ trợ chỉnh trang đô thị, cải tạo vỉa hè Quận 1 năm 2026",
                    Excerpt = "Chương trình nâng cấp, chỉnh trang các tuyến vỉa hè trung tâm nhằm nâng cao mỹ quan đô thị và tạo không gian đi bộ an toàn cho người dân.",
                    Content = "Nội dung chính sách chi tiết...",
                    Tag = "Chính sách",
                    TagClass = "tag-policy",
                    IssuingUnit = "UBND Quận 1",
                    PublishedDate = DateTime.UtcNow,
                    IsActive = true
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
